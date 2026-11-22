using GymManagement.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly GymDbContext _context;

    public ReportsController(GymDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var tomorrow = today.AddDays(1);

        var monthStart = new DateTime(
            now.Year,
            now.Month,
            1,
            0, 0, 0,
            DateTimeKind.Utc
        );

        var nextMonth = monthStart.AddMonths(1);

        // Total registered gym members
        var totalMembers = await _context.Members.CountAsync();

        // Active memberships that are currently valid
        var activeMemberships = await _context.Memberships
            .CountAsync(m =>
                m.Status == "Active" &&
                m.StartDate <= now &&
                m.EndDate >= now);

        // Total recorded payments
        var totalPayments = await _context.Payments.CountAsync();

        // Revenue from completed payments
        var totalRevenue = await _context.Payments
            .Where(p => p.Status == "Completed")
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        // Today's attendance check-ins
        var todayAttendance = await _context.Attendances
            .CountAsync(a =>
                a.CheckInTime >= today &&
                a.CheckInTime < tomorrow);

        // Current month's attendance check-ins
        var monthlyAttendance = await _context.Attendances
            .CountAsync(a =>
                a.CheckInTime >= monthStart &&
                a.CheckInTime < nextMonth);

        return Ok(new
        {
            totalMembers,
            activeMemberships,
            totalPayments,
            totalRevenue,
            todayAttendance,
            monthlyAttendance
        });
    }
}
