namespace Domain.Entities;

public class RecurringExpense : BaseEntity
{
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty; // Monthly, Weekly, Yearly
    public int DayOfMonth { get; set; } // يوم التكرار في الشهر
    public bool IsActive { get; set; } = true;
    public DateTime? LastProcessedDate { get; set; }

    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
