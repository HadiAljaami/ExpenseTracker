namespace Domain.Entities;

public class Expense : BaseEntity
{
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
