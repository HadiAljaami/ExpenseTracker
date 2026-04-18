namespace Application.DTOs.Budgets;

public class CreateBudgetDto
{
    public int? CategoryId { get; set; }
    public decimal MonthlyLimit { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}

public class BudgetResponseDto
{
    public int Id { get; set; }
    public string? CategoryName { get; set; }
    public decimal MonthlyLimit { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal Remaining => MonthlyLimit - TotalSpent;
    public double UsagePercentage => MonthlyLimit > 0 ? (double)(TotalSpent / MonthlyLimit) * 100 : 0;
    public bool IsExceeded => TotalSpent > MonthlyLimit;
    public int Month { get; set; }
    public int Year { get; set; }
}
