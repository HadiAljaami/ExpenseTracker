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
            var categoryLabel = budget.CategoryId.HasValue ? $" ({budget.Category?.Name})" : "";

            if (percentage >= 100)
            {
                var exists = await _alertRepo.ExistsAsync(userId, "BudgetExceeded", now.Month, now.Year, budget.Id);
                if (!exists)
                    await _alertRepo.AddAsync(new Alert
                    {
                        UserId = userId, BudgetId = budget.Id, Month = now.Month, Year = now.Year,
                        Title = "⚠️ Budget Exceeded",
                        Message = $"You have exceeded your{categoryLabel} budget of {budget.MonthlyLimit:C}. Total spent: {totalSpent:C}.",
                        Type = "BudgetExceeded"
                    });
            }
            else if (percentage >= 80)
            {
                var exists = await _alertRepo.ExistsAsync(userId, "BudgetWarning", now.Month, now.Year, budget.Id);
                if (!exists)
                    await _alertRepo.AddAsync(new Alert
                    {
                        UserId = userId, BudgetId = budget.Id, Month = now.Month, Year = now.Year,
                        Title = "🔔 Budget Warning",
                        Message = $"You have used {percentage:F0}% of your{categoryLabel} budget. Remaining: {(budget.MonthlyLimit - totalSpent):C}.",
                        Type = "BudgetWarning"
                    });
            }
        }

        var todayTotal = expenses.Where(e => e.Date.Date == now.Date).Sum(e => e.Amount);
        var daysElapsed = (now - from).Days + 1;
        var dailyAvg = daysElapsed > 1 ? expenses.Sum(e => e.Amount) / daysElapsed : 0;

        if (dailyAvg > 0 && todayTotal > dailyAvg * 2)
        {
            var spikeExists = await _alertRepo.ExistsAsync(userId, "SpendingSpike", now.Month, now.Year);
            if (!spikeExists)
                await _alertRepo.AddAsync(new Alert
                {
                    UserId = userId, Month = now.Month, Year = now.Year,
                    Title = "📈 Spending Spike Detected",
                    Message = $"You spent {todayTotal:C} today — more than double your daily average of {dailyAvg:C}.",
                    Type = "SpendingSpike"
                });
        }

        await _alertRepo.SaveChangesAsync();
    }

    public async Task<PagedAlertsDto> GetAlertsAsync(int userId, int page, int pageSize, bool? unreadOnly = null)
    {
        var (items, total, unread) = await _alertRepo.GetPagedAsync(userId, page, pageSize);
        var filtered = unreadOnly == true ? items.Where(a => !a.IsRead).ToList() : items;

        return new PagedAlertsDto
        {
            Items = filtered.Select(MapToDto).ToList(),
            TotalCount = unreadOnly == true ? unread : total,
            UnreadCount = unread,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<AlertDto>> GetAllAlertsAsync(int userId)
    {
        var alerts = await _alertRepo.GetAllAsync(userId);
        return alerts.Select(MapToDto).ToList();
    }

    public async Task<List<AlertDto>> GetUnreadAlertsAsync(int userId)
    {
        var alerts = await _alertRepo.GetUnreadAsync(userId);
        return alerts.Select(MapToDto).ToList();
    }

    public async Task MarkAsReadAsync(int userId, int alertId)
    {
        var alert = await _alertRepo.GetByIdAsync(alertId)
            ?? throw new KeyNotFoundException("Alert not found.");
        if (alert.UserId != userId) throw new UnauthorizedAccessException("Access denied.");
        alert.IsRead = true;
        await _alertRepo.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var alerts = await _alertRepo.GetUnreadAsync(userId);
        foreach (var alert in alerts) alert.IsRead = true;
        await _alertRepo.SaveChangesAsync();
    }

    private static AlertDto MapToDto(Alert a) => new()
    {
        Id = a.Id, Title = a.Title, Message = a.Message,
        Type = a.Type, IsRead = a.IsRead, CreatedAt = a.CreatedAt
    };
}
