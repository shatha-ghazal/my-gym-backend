
namespace GymManagement.API.Models;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Member";

    public int? MemberId { get; set; }

    public Member? Member { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
