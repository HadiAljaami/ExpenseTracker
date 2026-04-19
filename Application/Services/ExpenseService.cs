using Application.DTOs.Expenses;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

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
            ?? throw new NotFoundException("Category", dto.CategoryId);

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
        await _alertService.CheckAndCreateAlertsAsync(userId);

        return MapToResponse(expense, category);
    }

    public async Task<ExpenseResponseDto> UpdateAsync(int userId, int expenseId, UpdateExpenseDto dto)
    {
        var expense = await _expenseRepo.GetByIdAsync(expenseId)
            ?? throw new NotFoundException("Expense", expenseId);

        if (expense.UserId != userId)
            throw new ForbiddenException();

        var category = await _categoryRepo.GetByIdAsync(dto.CategoryId)
            ?? throw new NotFoundException("Category", dto.CategoryId);

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

    public async Task DeleteAsync(int userId, int expenseId)
    {
        var expense = await _expenseRepo.GetByIdAsync(expenseId)
            ?? throw new NotFoundException("Expense", expenseId);

        if (expense.UserId != userId)
            throw new ForbiddenException();

        await _expenseRepo.DeleteAsync(expense);
        await _expenseRepo.SaveChangesAsync();
    }

    public async Task RestoreAsync(int userId, int expenseId)
    {
        var expense = await _expenseRepo.GetByIdIncludeDeletedAsync(expenseId)
            ?? throw new NotFoundException("Expense", expenseId);

        if (expense.UserId != userId)
            throw new ForbiddenException();

        if (!expense.IsDeleted)
            throw new BusinessException("Expense is not deleted.");

        await _expenseRepo.RestoreAsync(expense);
        await _expenseRepo.SaveChangesAsync();
    }

    public async Task<ExpenseResponseDto> GetByIdAsync(int userId, int expenseId)
    {
        var expense = await _expenseRepo.GetByIdAsync(expenseId)
            ?? throw new NotFoundException("Expense", expenseId);

        if (expense.UserId != userId)
            throw new ForbiddenException();

        return MapToResponse(expense, expense.Category);
    }

    // ✅ Filtering في Database مباشرة - لا Memory Filtering
    public async Task<PagedExpensesDto> GetAllAsync(int userId, ExpenseFilterDto filter)
    {
        var (expenses, total) = await _expenseRepo.GetFilteredAsync(
            userId,
            filter.CategoryId,
            filter.FromDate,
            filter.ToDate,
            filter.MinAmount,
            filter.MaxAmount,
            filter.Search,
            filter.SortBy ?? "Date",
            filter.SortDirection ?? "desc",
            filter.Page,
            filter.PageSize);

        return new PagedExpensesDto
        {
            Items = expenses.Select(e => MapToResponse(e, e.Category)).ToList(),
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<List<ExpenseResponseDto>> GetDeletedAsync(int userId)
    {
        var expenses = await _expenseRepo.GetDeletedExpensesAsync(userId);
        return expenses.Select(e => MapToResponse(e, e.Category)).ToList();
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
