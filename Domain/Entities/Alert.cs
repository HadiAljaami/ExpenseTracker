namespace Domain.Entities;

public class Alert : BaseEntity
{
    public int UserId { get; set; }
    public int? BudgetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public bool IsRead { get; set; } = false;

    public User User { get; set; } = null!;
}
