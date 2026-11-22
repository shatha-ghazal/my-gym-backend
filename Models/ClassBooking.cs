namespace GymManagement.API.Models;

public class ClassBooking
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    public Member? Member { get; set; }

    public int GymClassId { get; set; }

    public GymClass? GymClass { get; set; }

    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Booked";
}