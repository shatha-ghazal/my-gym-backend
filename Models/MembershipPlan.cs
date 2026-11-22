namespace GymManagement.API.Models;

public class MembershipPlan
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DurationInMonths { get; set; }

    public bool IsActive { get; set; } = true;
}