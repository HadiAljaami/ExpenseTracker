using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByEmailAsync(string email) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetByIdAsync(int id) =>
        _db.Users.FindAsync(id).AsTask();

    public async Task AddAsync(User user) => await _db.Users.AddAsync(user);
    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

public class ExpenseRepository : IExpenseRepository
{
    private readonly AppDbContext _db;
    public ExpenseRepository(AppDbContext db) => _db = db;

    public Task<Expense?> GetByIdAsync(int id) =>
        _db.Expenses.Include(e => e.Category).FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<Expense>> GetUserExpensesAsync(int userId, DateTime? from = null, DateTime? to = null)
    {
        var query = _db.Expenses.Include(e => e.Category).Where(e => e.UserId == userId);
        if (from.HasValue) query = query.Where(e => e.Date >= from.Value);
        if (to.HasValue) query = query.Where(e => e.Date <= to.Value);
        return await query.ToListAsync();
    }

    public async Task AddAsync(Expense expense) => await _db.Expenses.AddAsync(expense);
    public Task UpdateAsync(Expense expense) { _db.Expenses.Update(expense); return Task.CompletedTask; }
    public Task DeleteAsync(Expense expense) { _db.Expenses.Remove(expense); return Task.CompletedTask; }
    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

public class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _db;
    public BudgetRepository(AppDbContext db) => _db = db;

    public Task<Budget?> GetByIdAsync(int id) =>
        _db.Budgets.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id);

    public Task<List<Budget>> GetUserBudgetsAsync(int userId, int month, int year) =>
        _db.Budgets.Include(b => b.Category)
            .Where(b => b.UserId == userId && b.Month == month && b.Year == year)
            .ToListAsync();

    public async Task AddAsync(Budget budget) => await _db.Budgets.AddAsync(budget);
    public Task DeleteAsync(Budget budget) { _db.Budgets.Remove(budget); return Task.CompletedTask; }
    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _db;
    public AlertRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Alert alert) => await _db.Alerts.AddAsync(alert);

    public Task<List<Alert>> GetUnreadAsync(int userId) =>
        _db.Alerts.Where(a => a.UserId == userId && !a.IsRead)
            .OrderByDescending(a => a.CreatedAt).ToListAsync();

    public Task<Alert?> GetByIdAsync(int id) => _db.Alerts.FindAsync(id).AsTask();
    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;
    public CategoryRepository(AppDbContext db) => _db = db;

    public Task<List<Category>> GetAllAsync() => _db.Categories.ToListAsync();
    public Task<Category?> GetByIdAsync(int id) => _db.Categories.FindAsync(id).AsTask();
}
