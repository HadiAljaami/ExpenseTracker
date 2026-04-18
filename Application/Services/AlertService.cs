using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepo;
    private readonly IExpenseRepository _expenseRepo;
    private readonly IBudgetRepository _budgetRepo;

    public AlertService(IAlertRepository alertRepo, IExpenseRepository expenseRepo, IBudgetRepository budgetRepo)
    {
        _alertRepo = alertRepo;
        _expenseRepo = expenseRepo;
        _budgetRepo = budgetRepo;
    }

    public async Task CheckAndCreateAlertsAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var from = new DateTime(now.Year, now.Month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var expenses = await _expenseRepo.GetUserExpensesAsync(userId, from, to);
        var budgets = await _budgetRepo.GetUserBudgetsAsync(userId, now.Month, now.Year);

        foreach (var budget in budgets)
        {
            var relevantExpenses = budget.CategoryId.HasValue
                ? expenses.Where(e => e.CategoryId == budget.CategoryId).ToList()
                : expenses;

            var totalSpent = relevantExpenses.Sum(e => e.Amount);
            var percentage = budget.MonthlyLimit > 0 ? (double)(totalSpent / budget.MonthlyLimit) * 100 : 0;

            if (percentage >= 100)
            {
                var categoryLabel = budget.CategoryId.HasValue
                    ? $" ({budget.Category?.Name})"
                    : "";

                await _alertRepo.AddAsync(new Alert
                {
                    UserId = userId,
                    Title = "⚠️ Budget Exceeded",
                    Message = $"You have exceeded your{categoryLabel} budget of {budget.MonthlyLimit:C} this month. Total spent: {totalSpent:C}.",
                    Type = "BudgetExceeded"
                });
            }
            else if (percentage >= 80)
            {
                var categoryLabel = budget.CategoryId.HasValue
                    ? $" ({budget.Category?.Name})"
                    : "";

                await _alertRepo.AddAsync(new Alert
                {
                    UserId = userId,
                    Title = "🔔 Budget Warning",
                    Message = $"You have used {percentage:F0}% of your{categoryLabel} budget. Remaining: {(budget.MonthlyLimit - totalSpent):C}.",
                    Type = "BudgetWarning"
                });
            }
        }

        // Spending spike: today vs daily average
        var todayTotal = expenses.Where(e => e.Date.Date == now.Date).Sum(e => e.Amount);
        var daysElapsed = (now - from).Days + 1;
        var dailyAvg = daysElapsed > 1 ? expenses.Sum(e => e.Amount) / daysElapsed : 0;

        if (dailyAvg > 0 && todayTotal > dailyAvg * 2)
        {
            await _alertRepo.AddAsync(new Alert
            {
                UserId = userId,
                Title = "📈 Spending Spike Detected",
                Message = $"You spent {todayTotal:C} today — more than double your daily average of {dailyAvg:C}.",
                Type = "SpendingSpike"
            });
        }

        await _alertRepo.SaveChangesAsync();
    }

    public async Task<List<AlertDto>> GetUnreadAlertsAsync(int userId)
    {
        var alerts = await _alertRepo.GetUnreadAsync(userId);
        return alerts.Select(a => new AlertDto
        {
            Id = a.Id,
            Title = a.Title,
            Message = a.Message,
            Type = a.Type,
            IsRead = a.IsRead,
            CreatedAt = a.CreatedAt
        }).ToList();
    }

    public async Task MarkAsReadAsync(int userId, int alertId)
    {
        var alert = await _alertRepo.GetByIdAsync(alertId)
            ?? throw new KeyNotFoundException("Alert not found.");

        if (alert.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        alert.IsRead = true;
        await _alertRepo.SaveChangesAsync();
    }
}
