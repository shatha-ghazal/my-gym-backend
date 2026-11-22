
using GymManagement.API.Data;
using GymManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipsController : ControllerBase
{
    private readonly GymDbContext _context;

    public MembershipsController(GymDbContext context)
    {
        _context = context;
    }

    // GET: api/memberships
    // GET: api/memberships
    // Admin only: View all gym membership records
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Membership>>> GetMemberships()
    {
        return await _context.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .ToListAsync();
    }

    // GET: api/memberships/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Membership>> GetMembership(int id)
    {
        var membership = await _context.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (membership == null)
        {
            return NotFound();
        }

        return membership;
    }

    // POST: api/memberships
    [HttpPost]
    public async Task<ActionResult<Membership>> CreateMembership(
        Membership membership)
    {
        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMembership),
            new { id = membership.Id },
            membership);
    }

    // PUT: api/memberships/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMembership(
        int id, Membership membership)
    {
        if (id != membership.Id)
        {
            return BadRequest();
        }

        _context.Entry(membership).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Memberships.AnyAsync(m => m.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    // DELETE: api/memberships/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMembership(int id)
    {
        var membership = await _context.Memberships.FindAsync(id);

        if (membership == null)
        {
            return NotFound();
        }

        _context.Memberships.Remove(membership);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
