using Application.DTOs.Users;
using Application.Interfaces;

namespace Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepo;
    private readonly IExpenseRepository _expenseRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IBudgetRepository _budgetRepo;

    public AdminService(IUserRepository userRepo, IExpenseRepository expenseRepo,
        ICategoryRepository categoryRepo, IBudgetRepository budgetRepo)
    {
        _userRepo = userRepo;
        _expenseRepo = expenseRepo;
        _categoryRepo = categoryRepo;
        _budgetRepo = budgetRepo;
    }

    public async Task<List<AdminUserDto>> GetAllUsersAsync()
    {
        var users = await _userRepo.GetAllAsync();
        var result = new List<AdminUserDto>();

        foreach (var user in users)
        {
            var expenses = await _expenseRepo.GetUserExpensesAsync(user.Id);
            result.Add(new AdminUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Currency = user.Currency,
                TotalExpenses = expenses.Count,
                TotalAmount = expenses.Sum(e => e.Amount),
                CreatedAt = user.CreatedAt
            });
        }

        return result.OrderByDescending(u => u.CreatedAt).ToList();
    }

    public async Task<AdminUserDto> GetUserByIdAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var expenses = await _expenseRepo.GetUserExpensesAsync(userId);
        return new AdminUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Currency = user.Currency,
            TotalExpenses = expenses.Count,
            TotalAmount = expenses.Sum(e => e.Amount),
            CreatedAt = user.CreatedAt
        };
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Role == "Admin")
            throw new InvalidOperationException("Cannot delete an admin user.");

        await _userRepo.DeleteAsync(user);
        await _userRepo.SaveChangesAsync();
    }

    public async Task<AdminStatsDto> GetSystemStatsAsync()
    {
        var users = await _userRepo.GetAllAsync();
        var allExpenses = await _expenseRepo.GetAllExpensesAsync();
        var categoryCount = await _categoryRepo.GetCountAsync();
        var activeBudgets = await _budgetRepo.GetActiveBudgetsCountAsync();

        var topSpenders = users
            .Select(async u =>
            {
                var expenses = await _expenseRepo.GetUserExpensesAsync(u.Id);
                return new TopSpenderDto
                {
                    FullName = u.FullName,
                    Email = u.Email,
                    TotalAmount = expenses.Sum(e => e.Amount),
                    ExpenseCount = expenses.Count
                };
            });

        var spenders = (await Task.WhenAll(topSpenders))
            .OrderByDescending(s => s.TotalAmount)
            .Take(5)
            .ToList();

        return new AdminStatsDto
        {
            TotalUsers = users.Count,
            TotalExpenses = allExpenses.Count,
            TotalAmount = allExpenses.Sum(e => e.Amount),
            TotalCategories = categoryCount,
            ActiveBudgets = activeBudgets,
            TopSpenders = spenders
        };
    }
}
