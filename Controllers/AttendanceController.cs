using System.Security.Claims;
using GymManagement.API.Data;
using GymManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly GymDbContext _context;

    public AttendanceController(GymDbContext context)
    {
        _context = context;
    }

    private async Task<Member?> GetAuthorizedMemberAsync()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int userId))
            return null;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.MemberId == null)
            return null;

        if (User.IsInRole("Admin"))
            return await _context.Members
                .FirstOrDefaultAsync(m => m.Id == user.MemberId);

        var today = DateTime.UtcNow;

        var hasMembership = await _context.Memberships
            .AnyAsync(m =>
                m.MemberId == user.MemberId &&
                m.Status == "Active" &&
                m.StartDate <= today &&
                m.EndDate >= today);

        if (!hasMembership)
            return null;

        return await _context.Members
            .FirstOrDefaultAsync(m =>
                m.Id == user.MemberId &&
                m.IsActive);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyAttendance()
    {
        var member = await GetAuthorizedMemberAsync();

        if (member == null)
            return Forbid();

        var records = await _context.Attendances
            .Where(a => a.MemberId == member.Id)
            .OrderByDescending(a => a.CheckInTime)
            .Select(a => new
            {
                a.Id,
                a.CheckInTime,
                a.CheckOutTime,
                a.Status
            })
            .ToListAsync();

        return Ok(records);
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn()
    {
        var member = await GetAuthorizedMemberAsync();

        if (member == null)
            return Forbid();

        var hasOpenAttendance = await _context.Attendances
            .AnyAsync(a =>
                a.MemberId == member.Id &&
                a.CheckOutTime == null);

        if (hasOpenAttendance)
            return BadRequest(new
            {
                message = "You are already checked in."
            });

        var attendance = new Attendance
        {
            MemberId = member.Id,
            CheckInTime = DateTime.UtcNow,
            Status = "Present"
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Checked in successfully.",
            attendance.Id,
            attendance.CheckInTime
        });
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut()
    {
        var member = await GetAuthorizedMemberAsync();

        if (member == null)
            return Forbid();

        var attendance = await _context.Attendances
            .Where(a =>
                a.MemberId == member.Id &&
                a.CheckOutTime == null)
            .OrderByDescending(a => a.CheckInTime)
            .FirstOrDefaultAsync();

        if (attendance == null)
            return BadRequest(new
            {
                message = "No active check-in was found."
            });

        attendance.CheckOutTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Checked out successfully.",
            attendance.Id,
            attendance.CheckOutTime
        });
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAttendance()
    {
        var records = await _context.Attendances
            .Include(a => a.Member)
            .OrderByDescending(a => a.CheckInTime)
            .Select(a => new
            {
                a.Id,
                a.MemberId,
                MemberName = a.Member == null
                    ? ""
                    : a.Member.FirstName + " " +
                      a.Member.LastName,
                a.CheckInTime,
                a.CheckOutTime,
                a.Status
            })
            .ToListAsync();

        return Ok(records);
    }
}
