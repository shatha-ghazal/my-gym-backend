namespace GymManagement.API.Models;

public class GymClass
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int TrainerId { get; set; }

    public Trainer? Trainer { get; set; }

    public DateTime ScheduleDate { get; set; }

    public int Capacity { get; set; }

    public bool IsActive { get; set; } = true;
}