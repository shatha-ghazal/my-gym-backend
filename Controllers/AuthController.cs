
using GymManagement.API.Services;
using Microsoft.AspNetCore.Mvc;
using GymManagement.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly GymDbContext _context;

        public AuthController(
            AuthService authService,
            GymDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new
                    {
                        message = "Email and password are required."
                    });
                }

                var token = await _authService.RegisterAsync(
                    request.Email,
                    request.Password);

                return Ok(new
                {
                    message = "Registration successful.",
                    token
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new
                    {
                        message = "Email and password are required."
                    });
                }

                var token = await _authService.LoginAsync(
                    request.Email,
                    request.Password);

                return Ok(new
                {
                    message = "Login successful.",
                    token
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }


        // GET: api/Auth/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user authentication."
                });
            }

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Member)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User profile not found."
                });
            }

            var member = user.Member;

            var membership = member == null
                ? null
                : await _context.Memberships
                    .AsNoTracking()
                    .Include(m => m.MembershipPlan)
                    .Where(m =>
                        m.MemberId == member.Id &&
                        m.Status == "Active" &&
                        m.StartDate <= DateTime.UtcNow &&
                        m.EndDate >= DateTime.UtcNow)
                    .OrderByDescending(m => m.EndDate)
                    .FirstOrDefaultAsync();

            return Ok(new
            {
                email = user.Email,
                firstName = member?.FirstName ?? "",
                lastName = member?.LastName ?? "",
                phoneNumber = member?.PhoneNumber ?? "",
                dateJoined = member?.DateJoined ?? user.CreatedAt,
                membershipPlan = membership?.MembershipPlan?.Name,
                membershipStatus = membership == null
                    ? "No Active Membership"
                    : "Active",
                                membershipStartDate = membership?.StartDate,
                membershipEndDate = membership?.EndDate
            });
        }

        // PUT: api/Auth/profile
        
        // PUT: api/Auth/profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user authentication."
                });
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new
                {
                    message = "First name and last name are required."
                });
            }

            var user = await _context.Users
                .Include(u => u.Member)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            if (user.Member == null)
            {
                return BadRequest(new
                {
                    message = "No member profile is linked to this account."
                });
            }

            user.Member.FirstName = request.FirstName.Trim();
            user.Member.LastName = request.LastName.Trim();
            user.Member.PhoneNumber =
                request.PhoneNumber?.Trim() ?? "";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully!"
            });
        }


    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

        public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
    }
}

