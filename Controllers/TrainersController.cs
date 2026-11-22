using GymManagement.API.Data;
using GymManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainersController : ControllerBase
{
    private readonly GymDbContext _context;

    public TrainersController(GymDbContext context)
    {
        _context = context;
    }

    // GET: api/trainers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Trainer>>> GetTrainers()
    {
        return await _context.Trainers.ToListAsync();
    }

    // GET: api/trainers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Trainer>> GetTrainer(int id)
    {
        var trainer = await _context.Trainers.FindAsync(id);

        if (trainer == null)
        {
            return NotFound();
        }

        return trainer;
    }

    
    // POST: api/trainers
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Trainer>> CreateTrainer(Trainer trainer)
    {
        _context.Trainers.Add(trainer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTrainer),
            new { id = trainer.Id },
            trainer);
    }

    // PUT: api/trainers/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTrainer(int id, Trainer trainer)
    {
        if (id != trainer.Id)
        {
            return BadRequest();
        }

        _context.Entry(trainer).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Trainers.AnyAsync(t => t.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    // DELETE: api/trainers/5
    // DELETE: api/trainers/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTrainer(int id)
    {
        var trainer = await _context.Trainers.FindAsync(id);

        if (trainer == null)
        {
            return NotFound();
        }

        _context.Trainers.Remove(trainer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}