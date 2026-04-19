namespace Application.DTOs.RecurringExpenses;

public class CreateRecurringExpenseDto
{
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Frequency { get; set; } = "Monthly"; // Monthly, Weekly, Yearly
    public int DayOfMonth { get; set; } = 1;
}

public class RecurringExpenseResponseDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int DayOfMonth { get; set; }
    public bool IsActive { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public DateTime? LastProcessedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
