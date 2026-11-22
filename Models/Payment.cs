namespace GymManagement.API.Models;

public class Payment
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    public Member? Member { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public string PaymentMethod { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public string TransactionReference { get; set; } = string.Empty;
}