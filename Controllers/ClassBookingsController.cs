using GymManagement.API.Data;
using GymManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassBookingsController : ControllerBase
{
    private readonly GymDbContext _context;

    public ClassBookingsController(GymDbContext context)
    {
        _context = context;
    }

    // GET: api/classbookings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClassBooking>>> GetClassBookings()
    {
        return await _context.ClassBookings
            .Include(b => b.Member)
            .Include(b => b.GymClass)
            .ToListAsync();
    }

    // GET: api/classbookings/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ClassBooking>> GetClassBooking(int id)
    {
        var booking = await _context.ClassBookings
            .Include(b => b.Member)
            .Include(b => b.GymClass)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            return NotFound();
        }

        return booking;
    }

    // POST: api/classbookings
    [HttpPost]
    public async Task<ActionResult<ClassBooking>> CreateClassBooking(
        ClassBooking booking)
    {
        _context.ClassBookings.Add(booking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetClassBooking),
            new { id = booking.Id },
            booking);
    }

    // PUT: api/classbookings/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClassBooking(
        int id, ClassBooking booking)
    {
        if (id != booking.Id)
        {
            return BadRequest();
        }

        _context.Entry(booking).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.ClassBookings.AnyAsync(b => b.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    // DELETE: api/classbookings/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClassBooking(int id)
    {
        var booking = await _context.ClassBookings.FindAsync(id);

        if (booking == null)
        {
            return NotFound();
        }

        _context.ClassBookings.Remove(booking);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}