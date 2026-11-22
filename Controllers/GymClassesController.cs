using GymManagement.API.Data;
using GymManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GymClassesController : ControllerBase
{
    private readonly GymDbContext _context;

    public GymClassesController(GymDbContext context)
    {
        _context = context;
    }

    // GET: api/gymclasses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GymClass>>> GetGymClasses()
    {
        return await _context.GymClasses
            .Include(g => g.Trainer)
            .ToListAsync();
    }

    // GET: api/gymclasses/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GymClass>> GetGymClass(int id)
    {
        var gymClass = await _context.GymClasses
            .Include(g => g.Trainer)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gymClass == null)
        {
            return NotFound();
        }

        return gymClass;
    }

    // POST: api/gymclasses
    [HttpPost]
    public async Task<ActionResult<GymClass>> CreateGymClass(GymClass gymClass)
    {
        _context.GymClasses.Add(gymClass);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetGymClass),
            new { id = gymClass.Id },
            gymClass);
    }

    // PUT: api/gymclasses/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGymClass(int id, GymClass gymClass)
    {
        if (id != gymClass.Id)
        {
            return BadRequest();
        }

        _context.Entry(gymClass).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.GymClasses.AnyAsync(g => g.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    // DELETE: api/gymclasses/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGymClass(int id)
    {
        var gymClass = await _context.GymClasses.FindAsync(id);

        if (gymClass == null)
        {
            return NotFound();
        }

        _context.GymClasses.Remove(gymClass);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}