namespace Application.DTOs.Reports;

public class MonthlyReportDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalExpenses { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal BudgetRemaining => TotalBudget - TotalExpenses;
    public bool OverBudget => TotalExpenses > TotalBudget;
    public int TransactionCount { get; set; }
    public List<CategoryReportDto> Categories { get; set; } = new();
    public List<WeeklyBreakdownDto> WeeklyBreakdown { get; set; } = new();
}

public class CategoryReportDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
    public int TransactionCount { get; set; }
}

public class WeeklyBreakdownDto
{
    public int WeekNumber { get; set; }
    public decimal Amount { get; set; }
}
