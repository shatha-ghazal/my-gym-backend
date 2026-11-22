using System.Security.Claims;
using GymManagement.API.Data;
using GymManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/membership-checkout")]
[Authorize]
public class MembershipCheckoutController : ControllerBase
{
    private readonly GymDbContext _context;

    public MembershipCheckoutController(GymDbContext context)
    {
        _context = context;
    }

    public class CheckoutRequest
    {
        public int MembershipPlanId { get; set; }
    }

    // GET: api/membership-checkout/status
    [HttpGet("status")]
    public async Task<IActionResult> GetMembershipStatus()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (user == null)
            return Unauthorized();

        if (user.Role == "Admin")
        {
            return Ok(new
            {
                isActive = true,
                isAdmin = true
            });
        }

        if (user.MemberId == null)
        {
            return Ok(new
            {
                isActive = false,
                isAdmin = false
            });
        }

        var now = DateTime.UtcNow;

        var activeMembership = await _context.Memberships
            .AsNoTracking()
            .Where(m =>
                m.MemberId == user.MemberId.Value &&
                m.Status == "Active" &&
                m.StartDate <= now &&
                m.EndDate > now)
            .OrderByDescending(m => m.EndDate)
            .Select(m => new
            {
                m.Id,
                m.MembershipPlanId,
                m.StartDate,
                m.EndDate
            })
            .FirstOrDefaultAsync();

        return Ok(new
        {
            isActive = activeMembership != null,
            isAdmin = false,
            membership = activeMembership
        });
    }

    // POST: api/membership-checkout/confirm
    // DEMO ONLY: no real money is collected.
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmCheckout(
        [FromBody] CheckoutRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        if (request.MembershipPlanId <= 0)
            return BadRequest(new
            {
                message = "Select a valid membership plan."
            });

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (user == null)
            return Unauthorized();

        if (user.Role == "Admin")
            return Forbid();

        var plan = await _context.MembershipPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.Id == request.MembershipPlanId &&
                p.IsActive);

        if (plan == null ||
            plan.Price < 0 ||
            plan.DurationInMonths <= 0)
        {
            return BadRequest(new
            {
                message = "The selected plan is unavailable."
            });
        }

        var now = DateTime.UtcNow;

        Member? member = null;

        if (user.MemberId.HasValue)
        {
            member = await _context.Members
                .FirstOrDefaultAsync(m =>
                    m.Id == user.MemberId.Value);
        }

        if (member == null)
        {
            member = new Member
            {
                Email = user.Email,
                DateJoined = now,
                IsActive = false
            };

            _context.Members.Add(member);
            user.Member = member;

            await _context.SaveChangesAsync();
        }

        var existingMembership = await _context.Memberships
            .AnyAsync(m =>
                m.MemberId == member.Id &&
                m.Status == "Active" &&
                m.EndDate > now);

        if (existingMembership)
        {
            return Conflict(new
            {
                message = "You already have an active membership."
            });
        }

        var payment = new Payment
        {
            MemberId = member.Id,
            Amount = plan.Price,
            PaymentDate = now,
            PaymentMethod = "Demo",
            Status = "Completed",
            TransactionReference =
                $"DEMO-{Guid.NewGuid():N}"
        };

        var membership = new Membership
        {
            MemberId = member.Id,
            MembershipPlanId = plan.Id,
            StartDate = now,
            EndDate = now.AddMonths(plan.DurationInMonths),
            Status = "Active"
        };

        member.IsActive = true;

        _context.Payments.Add(payment);
        _context.Memberships.Add(membership);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = "Demo payment completed. Membership activated.",
            paymentId = payment.Id,
            membershipId = membership.Id,
            planName = plan.Name,
            amount = payment.Amount,
            startDate = membership.StartDate,
            endDate = membership.EndDate,
            isActive = true
        });
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(value, out var id)
            ? id
            : null;
    }
}
