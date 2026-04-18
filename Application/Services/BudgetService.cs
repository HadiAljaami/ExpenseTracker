using Application.DTOs.Budgets;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepo;
    private readonly IExpenseRepository _expenseRepo;
    private readonly ICategoryRepository _categoryRepo;

    public BudgetService(IBudgetRepository budgetRepo, IExpenseRepository expenseRepo, ICategoryRepository categoryRepo)
    {
        _budgetRepo = budgetRepo;
        _expenseRepo = expenseRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<BudgetResponseDto> CreateAsync(int userId, CreateBudgetDto dto)
    {
        var budget = new Budget
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            MonthlyLimit = dto.MonthlyLimit,
            Month = dto.Month,
            Year = dto.Year
        };

        await _budgetRepo.AddAsync(budget);
        await _budgetRepo.SaveChangesAsync();

        return await EnrichBudget(budget);
    }

    public async Task<List<BudgetResponseDto>> GetAllAsync(int userId, int month, int year)
    {
        var budgets = await _budgetRepo.GetUserBudgetsAsync(userId, month, year);
        var result = new List<BudgetResponseDto>();

        foreach (var b in budgets)
            result.Add(await EnrichBudget(b));

        return result;
    }

    public async Task DeleteAsync(int userId, int budgetId)
    {
        var budget = await _budgetRepo.GetByIdAsync(budgetId)
            ?? throw new KeyNotFoundException("Budget not found.");

        if (budget.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        await _budgetRepo.DeleteAsync(budget);
        await _budgetRepo.SaveChangesAsync();
    }

    private async Task<BudgetResponseDto> EnrichBudget(Budget budget)
    {
        var from = new DateTime(budget.Year, budget.Month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var expenses = await _expenseRepo.GetUserExpensesAsync(budget.UserId, from, to);

        if (budget.CategoryId.HasValue)
            expenses = expenses.Where(e => e.CategoryId == budget.CategoryId).ToList();

        var spent = expenses.Sum(e => e.Amount);

        string? categoryName = null;
        if (budget.CategoryId.HasValue)
        {
            var cat = await _categoryRepo.GetByIdAsync(budget.CategoryId.Value);
            categoryName = cat?.Name;
        }

        return new BudgetResponseDto
        {
            Id = budget.Id,
            CategoryName = categoryName,
            MonthlyLimit = budget.MonthlyLimit,
            TotalSpent = spent,
            Month = budget.Month,
            Year = budget.Year
        };
    }
}
