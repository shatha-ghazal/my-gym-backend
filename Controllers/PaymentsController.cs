using GymManagement.API.Data;
using GymManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PaymentsController : ControllerBase
{
    private readonly GymDbContext _context;

    public PaymentsController(GymDbContext context)
    {
        _context = context;
    }

    // GET: api/payments
    // Admin only
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
    {
        return await _context.Payments
            .Include(p => p.Member)
            .ToListAsync();
    }

    // GET: api/payments/5
    // Admin only
    [HttpGet("{id}")]
    public async Task<ActionResult<Payment>> GetPayment(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Member)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null)
        {
            return NotFound();
        }

        return payment;
    }

    // POST: api/payments
    // Admin only
    [HttpPost]
    public async Task<ActionResult<Payment>> CreatePayment(
        Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetPayment),
            new { id = payment.Id },
            payment);
    }

    // PUT: api/payments/5
    // Admin only
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayment(
        int id, Payment payment)
    {
        if (id != payment.Id)
        {
            return BadRequest();
        }

        _context.Entry(payment).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Payments.AnyAsync(p => p.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    // DELETE: api/payments/5
    // Admin only
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayment(int id)
    {
        var payment = await _context.Payments.FindAsync(id);

        if (payment == null)
        {
            return NotFound();
        }

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
