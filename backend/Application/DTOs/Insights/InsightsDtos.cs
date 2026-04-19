namespace Application.DTOs.Insights;

public class InsightsSummaryDto
{
    public decimal TotalThisMonth { get; set; }
    public decimal TotalLastMonth { get; set; }
    public decimal ChangePercentage => TotalLastMonth > 0
        ? Math.Round((TotalThisMonth - TotalLastMonth) / TotalLastMonth * 100, 2)
        : 0;
    public decimal AverageDailySpending { get; set; }
    public string HighestSpendingCategory { get; set; } = string.Empty;
    public decimal HighestCategoryAmount { get; set; }
    public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
    public List<DailySpendingDto> DailyTrend { get; set; } = new();
}

public class CategoryBreakdownDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public double Percentage { get; set; }
}

public class DailySpendingDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}
