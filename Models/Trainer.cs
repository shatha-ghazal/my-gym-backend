namespace GymManagement.API.Models;

public class Trainer
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}