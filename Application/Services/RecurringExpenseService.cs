using Application.DTOs.RecurringExpenses;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class RecurringExpenseService : IRecurringExpenseService
{
    private readonly IRecurringExpenseRepository _recurringRepo;
    private readonly IExpenseRepository _expenseRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IAlertService _alertService;

    public RecurringExpenseService(IRecurringExpenseRepository recurringRepo,
        IExpenseRepository expenseRepo, ICategoryRepository categoryRepo, IAlertService alertService)
    {
        _recurringRepo = recurringRepo;
        _expenseRepo = expenseRepo;
        _categoryRepo = categoryRepo;
        _alertService = alertService;
    }

    public async Task<RecurringExpenseResponseDto> CreateAsync(int userId, CreateRecurringExpenseDto dto)
    {
        var category = await _categoryRepo.GetByIdAsync(dto.CategoryId)
            ?? throw new KeyNotFoundException("Category not found.");

        var recurring = new RecurringExpense
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            Description = dto.Description,
            Frequency = dto.Frequency,
            DayOfMonth = dto.DayOfMonth,
            IsActive = true
        };

        await _recurringRepo.AddAsync(recurring);
        await _recurringRepo.SaveChangesAsync();
        return MapToDto(recurring, category);
    }

    public async Task<List<RecurringExpenseResponseDto>> GetAllAsync(int userId)
    {
        var items = await _recurringRepo.GetUserRecurringExpensesAsync(userId);
        return items.Select(r => MapToDto(r, r.Category)).ToList();
    }

    public async Task<RecurringExpenseResponseDto> ToggleActiveAsync(int userId, int id)
    {
        var recurring = await _recurringRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Recurring expense not found.");

        if (recurring.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        recurring.IsActive = !recurring.IsActive;
        await _recurringRepo.UpdateAsync(recurring);
        await _recurringRepo.SaveChangesAsync();
        return MapToDto(recurring, recurring.Category);
    }

    public async Task DeleteAsync(int userId, int id)
    {
        var recurring = await _recurringRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Recurring expense not found.");

        if (recurring.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        await _recurringRepo.DeleteAsync(recurring);
        await _recurringRepo.SaveChangesAsync();
    }

    public async Task ProcessDueRecurringExpensesAsync()
    {
        var due = await _recurringRepo.GetDueRecurringExpensesAsync();

        foreach (var recurring in due)
        {
            var expense = new Expense
            {
                UserId = recurring.UserId,
                CategoryId = recurring.CategoryId,
                Amount = recurring.Amount,
                Description = $"[Auto] {recurring.Description}",
                Date = DateTime.UtcNow
            };

            await _expenseRepo.AddAsync(expense);
            recurring.LastProcessedDate = DateTime.UtcNow;
            await _recurringRepo.UpdateAsync(recurring);
            await _alertService.CheckAndCreateAlertsAsync(recurring.UserId);
        }

        await _expenseRepo.SaveChangesAsync();
        await _recurringRepo.SaveChangesAsync();
    }

    private static RecurringExpenseResponseDto MapToDto(RecurringExpense r, Category? category) => new()
    {
        Id = r.Id,
        Amount = r.Amount,
        Description = r.Description,
        Frequency = r.Frequency,
        DayOfMonth = r.DayOfMonth,
        IsActive = r.IsActive,
        CategoryName = category?.Name ?? "",
        CategoryIcon = category?.Icon ?? "",
        LastProcessedDate = r.LastProcessedDate,
        CreatedAt = r.CreatedAt
    };
}
