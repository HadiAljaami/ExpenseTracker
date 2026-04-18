namespace Domain.Entities;

public class Budget : BaseEntity
{
    public int UserId { get; set; }
    public int? CategoryId { get; set; }
    public decimal MonthlyLimit { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    public User User { get; set; } = null!;
    public Category? Category { get; set; }
}
