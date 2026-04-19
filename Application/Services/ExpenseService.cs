using Application.DTOs.Expenses;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IAlertService _alertService;

    public ExpenseService(IExpenseRepository expenseRepo, ICategoryRepository categoryRepo, IAlertService alertService)
    {
        _expenseRepo = expenseRepo;
        _categoryRepo = categoryRepo;
        _alertService = alertService;
    }

    public async Task<ExpenseResponseDto> CreateAsync(int userId, CreateExpenseDto dto)
    {
        var category = await _categoryRepo.GetByIdAsync(dto.CategoryId)
            ?? throw new KeyNotFoundException("Category not found.");

        var expense = new Expense
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            Description = dto.Description,
            Date = dto.Date
        };

        await _expenseRepo.AddAsync(expense);
        await _expenseRepo.SaveChangesAsync();

        // Check budget alerts after adding expense
        await _alertService.CheckAndCreateAlertsAsync(userId);

        return MapToResponse(expense, category);
    }

    public async Task<ExpenseResponseDto> UpdateAsync(int userId, int expenseId, UpdateExpenseDto dto)
    {
        var expense = await _expenseRepo.GetByIdAsync(expenseId)
            ?? throw new KeyNotFoundException("Expense not found.");

        if (expense.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        var category = await _categoryRepo.GetByIdAsync(dto.CategoryId)
            ?? throw new KeyNotFoundException("Category not found.");

        expense.CategoryId = dto.CategoryId;
        expense.Amount = dto.Amount;
        expense.Description = dto.Description;
        expense.Date = dto.Date;
        expense.UpdatedAt = DateTime.UtcNow;

        await _expenseRepo.UpdateAsync(expense);
        await _expenseRepo.SaveChangesAsync();
        await _alertService.CheckAndCreateAlertsAsync(userId);

        return MapToResponse(expense, category);
    }

    public async Task RestoreAsync(int userId, int expenseId)
    {
        var expense = await _expenseRepo.GetByIdIncludeDeletedAsync(expenseId)
            ?? throw new KeyNotFoundException("Expense not found.");

        if (expense.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        if (!expense.IsDeleted)
            throw new InvalidOperationException("Expense is not deleted.");

        await _expenseRepo.RestoreAsync(expense);
        await _expenseRepo.SaveChangesAsync();
    }

    public async Task<List<ExpenseResponseDto>> GetDeletedAsync(int userId)
    {
        var expenses = await _expenseRepo.GetDeletedExpensesAsync(userId);
        return expenses.Select(e => MapToResponse(e, e.Category)).ToList();
    }

    public async Task DeleteAsync(int userId, int expenseId)
    {
        var expense = await _expenseRepo.GetByIdAsync(expenseId)
            ?? throw new KeyNotFoundException("Expense not found.");

        if (expense.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        await _expenseRepo.DeleteAsync(expense);
        await _expenseRepo.SaveChangesAsync();
    }

    public async Task<ExpenseResponseDto> GetByIdAsync(int userId, int expenseId)
    {
        var expense = await _expenseRepo.GetByIdAsync(expenseId)
            ?? throw new KeyNotFoundException("Expense not found.");

        if (expense.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        return MapToResponse(expense, expense.Category);
    }

    public async Task<PagedExpensesDto> GetAllAsync(int userId, ExpenseFilterDto filter)
    {
        var expenses = await _expenseRepo.GetUserExpensesAsync(userId, filter.FromDate, filter.ToDate);

        if (filter.CategoryId.HasValue)
            expenses = expenses.Where(e => e.CategoryId == filter.CategoryId).ToList();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            expenses = expenses.Where(e => e.Description.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)).ToList();

        if (filter.MinAmount.HasValue)
            expenses = expenses.Where(e => e.Amount >= filter.MinAmount).ToList();

        if (filter.MaxAmount.HasValue)
            expenses = expenses.Where(e => e.Amount <= filter.MaxAmount).ToList();

        // Sorting
        expenses = filter.SortBy?.ToLower() switch
        {
            "amount" => filter.SortDirection == "asc"
                ? expenses.OrderBy(e => e.Amount).ToList()
                : expenses.OrderByDescending(e => e.Amount).ToList(),
            _ => filter.SortDirection == "asc"
                ? expenses.OrderBy(e => e.Date).ToList()
                : expenses.OrderByDescending(e => e.Date).ToList()
        };

        var total = expenses.Count;
        var items = expenses
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => MapToResponse(e, e.Category))
            .ToList();

        return new PagedExpensesDto
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    private static ExpenseResponseDto MapToResponse(Expense expense, Category category) => new()
    {
        Id = expense.Id,
        Amount = expense.Amount,
        Description = expense.Description,
        Date = expense.Date,
        CategoryName = category.Name,
        CategoryIcon = category.Icon,
        CategoryColor = category.Color,
        CreatedAt = expense.CreatedAt
    };
}
