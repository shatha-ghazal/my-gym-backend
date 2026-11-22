using GymManagement.API.Data;
using GymManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MembersController : ControllerBase
{
    private readonly GymDbContext _context;

    public MembersController(GymDbContext context)
    {
        _context = context;
    }

    // GET: api/members
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
    {
        return await _context.Members.ToListAsync();
    }

    // GET: api/members/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(int id)
    {
        var member = await _context.Members.FindAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        return member;
    }

    // POST: api/members
    [HttpPost]
    public async Task<ActionResult<Member>> CreateMember(Member member)
    {
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMember),
            new { id = member.Id },
            member);
    }

    // PUT: api/members/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(int id, Member member)
    {
        if (id != member.Id)
        {
            return BadRequest();
        }

        _context.Entry(member).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Members.AnyAsync(m => m.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    // DELETE: api/members/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var member = await _context.Members.FindAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        _context.Members.Remove(member);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}