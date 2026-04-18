using Application.DTOs.Reports;
using Application.Interfaces;

namespace Application.Services;

public class ReportService : IReportService
{
    private readonly IExpenseRepository _expenseRepo;
    private readonly IBudgetRepository _budgetRepo;

    public ReportService(IExpenseRepository expenseRepo, IBudgetRepository budgetRepo)
    {
        _expenseRepo = expenseRepo;
        _budgetRepo = budgetRepo;
    }

    public async Task<MonthlyReportDto> GetMonthlyReportAsync(int userId, int month, int year)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var expenses = await _expenseRepo.GetUserExpensesAsync(userId, from, to);
        var budgets = await _budgetRepo.GetUserBudgetsAsync(userId, month, year);

        var total = expenses.Sum(e => e.Amount);
        var totalBudget = budgets.Sum(b => b.MonthlyLimit);

        // Category breakdown
        var categories = expenses
            .GroupBy(e => e.Category)
            .Select(g => new CategoryReportDto
            {
                CategoryName = g.Key.Name,
                CategoryIcon = g.Key.Icon,
                Amount = g.Sum(e => e.Amount),
                Percentage = total > 0 ? Math.Round((double)(g.Sum(e => e.Amount) / total) * 100, 2) : 0,
                TransactionCount = g.Count()
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        // Weekly breakdown
        var weekly = expenses
            .GroupBy(e => GetWeekOfMonth(e.Date))
            .Select(g => new WeeklyBreakdownDto
            {
                WeekNumber = g.Key,
                Amount = g.Sum(e => e.Amount)
            })
            .OrderBy(w => w.WeekNumber)
            .ToList();

        return new MonthlyReportDto
        {
            Month = month,
            Year = year,
            MonthName = new DateTime(year, month, 1).ToString("MMMM yyyy"),
            TotalExpenses = total,
            TotalBudget = totalBudget,
            TransactionCount = expenses.Count,
            Categories = categories,
            WeeklyBreakdown = weekly
        };
    }

    private static int GetWeekOfMonth(DateTime date)
    {
        var firstDay = new DateTime(date.Year, date.Month, 1);
        return (int)Math.Ceiling((date.Day + (int)firstDay.DayOfWeek) / 7.0);
    }
}
