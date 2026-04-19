namespace Application.DTOs.Reports;

public class YearlyReportDto
{
    public int Year { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal AverageMonthlySpending { get; set; }
    public string HighestSpendingMonth { get; set; } = string.Empty;
    public string LowestSpendingMonth { get; set; } = string.Empty;
    public List<MonthlyBreakdownDto> MonthlyBreakdown { get; set; } = new();
    public List<CategoryReportDto> TopCategories { get; set; } = new();
}

public class MonthlyBreakdownDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalExpenses { get; set; }
    public decimal TotalBudget { get; set; }
    public bool OverBudget => TotalExpenses > TotalBudget;
}
