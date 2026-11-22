using GymManagement.API.Data;
using GymManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipPlansController : ControllerBase
{
    private readonly GymDbContext _context;

    public MembershipPlansController(GymDbContext context)
    {
        _context = context;
    }

    // Anyone can view active membership plans
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MembershipPlan>>> GetMembershipPlans()
    {
        return await _context.MembershipPlans
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    // Anyone can view an active membership plan
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<MembershipPlan>> GetMembershipPlan(int id)
    {
        var plan = await _context.MembershipPlans
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (plan == null)
        {
            return NotFound();
        }

        return plan;
    }

    // Admin only: Create a membership plan
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MembershipPlan>> CreateMembershipPlan(
        MembershipPlan plan)
    {
        _context.MembershipPlans.Add(plan);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMembershipPlan),
            new { id = plan.Id },
            plan);
    }

    // Admin only: Update a membership plan
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMembershipPlan(
        int id, MembershipPlan plan)
    {
        if (id != plan.Id)
        {
            return BadRequest();
        }

        _context.Entry(plan).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.MembershipPlans.AnyAsync(p => p.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    // Admin only: Delete a membership plan
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMembershipPlan(int id)
    {
        var plan = await _context.MembershipPlans.FindAsync(id);

        if (plan == null)
        {
            return NotFound();
        }

        _context.MembershipPlans.Remove(plan);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
