using Application.DTOs.Insights;
using Application.Interfaces;

namespace Application.Services;

public class InsightsService : IInsightsService
{
    private readonly IExpenseRepository _expenseRepo;

    public InsightsService(IExpenseRepository expenseRepo)
    {
        _expenseRepo = expenseRepo;
    }

    public async Task<InsightsSummaryDto> GetSummaryAsync(int userId, int month, int year)
    {
        var fromThis = new DateTime(year, month, 1);
        var toThis = fromThis.AddMonths(1).AddDays(-1);

        var lastMonth = fromThis.AddMonths(-1);
        var fromLast = new DateTime(lastMonth.Year, lastMonth.Month, 1);
        var toLast = fromLast.AddMonths(1).AddDays(-1);

        var thisMonthExpenses = await _expenseRepo.GetUserExpensesAsync(userId, fromThis, toThis);
        var lastMonthExpenses = await _expenseRepo.GetUserExpensesAsync(userId, fromLast, toLast);

        var totalThis = thisMonthExpenses.Sum(e => e.Amount);
        var totalLast = lastMonthExpenses.Sum(e => e.Amount);

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var avgDaily = totalThis / daysInMonth;

        // Category breakdown
        var categoryBreakdown = thisMonthExpenses
            .GroupBy(e => e.Category)
            .Select(g => new CategoryBreakdownDto
            {
                CategoryName = g.Key.Name,
                CategoryIcon = g.Key.Icon,
                CategoryColor = g.Key.Color,
                TotalAmount = g.Sum(e => e.Amount),
                TransactionCount = g.Count(),
                Percentage = totalThis > 0 ? Math.Round((double)(g.Sum(e => e.Amount) / totalThis) * 100, 2) : 0
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        var highest = categoryBreakdown.FirstOrDefault();

        // Daily trend
        var dailyTrend = thisMonthExpenses
            .GroupBy(e => e.Date.Date)
            .Select(g => new DailySpendingDto
            {
                Date = g.Key,
                Amount = g.Sum(e => e.Amount)
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new InsightsSummaryDto
        {
            TotalThisMonth = totalThis,
            TotalLastMonth = totalLast,
            AverageDailySpending = Math.Round(avgDaily, 2),
            HighestSpendingCategory = highest?.CategoryName ?? "N/A",
            HighestCategoryAmount = highest?.TotalAmount ?? 0,
            CategoryBreakdown = categoryBreakdown,
            DailyTrend = dailyTrend
        };
    }
}
