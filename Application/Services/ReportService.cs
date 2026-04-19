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

    public async Task<YearlyReportDto> GetYearlyReportAsync(int userId, int year)
    {
        var from = new DateTime(year, 1, 1);
        var to = new DateTime(year, 12, 31);

        var expenses = await _expenseRepo.GetUserExpensesAsync(userId, from, to);
        var total = expenses.Sum(e => e.Amount);

        var monthlyBreakdown = new List<MonthlyBreakdownDto>();

        for (int m = 1; m <= 12; m++)
        {
            var monthExpenses = expenses.Where(e => e.Date.Month == m).Sum(e => e.Amount);
            var budgets = await _budgetRepo.GetUserBudgetsAsync(userId, m, year);
            var monthBudget = budgets.Sum(b => b.MonthlyLimit);

            monthlyBreakdown.Add(new MonthlyBreakdownDto
            {
                Month = m,
                MonthName = new DateTime(year, m, 1).ToString("MMMM"),
                TotalExpenses = monthExpenses,
                TotalBudget = monthBudget
            });
        }

        var highest = monthlyBreakdown.OrderByDescending(m => m.TotalExpenses).First();
        var lowest = monthlyBreakdown.Where(m => m.TotalExpenses > 0).OrderBy(m => m.TotalExpenses).FirstOrDefault();

        var topCategories = expenses
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
            .Take(5)
            .ToList();

        return new YearlyReportDto
        {
            Year = year,
            TotalExpenses = total,
            TotalBudget = monthlyBreakdown.Sum(m => m.TotalBudget),
            AverageMonthlySpending = Math.Round(total / 12, 2),
            HighestSpendingMonth = highest.MonthName,
            LowestSpendingMonth = lowest?.MonthName ?? "N/A",
            MonthlyBreakdown = monthlyBreakdown,
            TopCategories = topCategories
        };
    }

    private static int GetWeekOfMonth(DateTime date)
    {
        var firstDay = new DateTime(date.Year, date.Month, 1);
        return (int)Math.Ceiling((date.Day + (int)firstDay.DayOfWeek) / 7.0);
    }
}
