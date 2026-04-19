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

    public Task<List<User>> GetAllAsync() =>
        _db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();

    public async Task AddAsync(User user) => await _db.Users.AddAsync(user);
    public Task DeleteAsync(User user) { _db.Users.Remove(user); return Task.CompletedTask; }
    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

public class ExpenseRepository : IExpenseRepository
{
    private readonly AppDbContext _db;
    public ExpenseRepository(AppDbContext db) => _db = db;

    public Task<Expense?> GetByIdAsync(int id) =>
        _db.Expenses.Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

    public Task<Expense?> GetByIdIncludeDeletedAsync(int id) =>
        _db.Expenses.Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<Expense>> GetUserExpensesAsync(int userId, DateTime? from = null, DateTime? to = null)
    {
        var query = _db.Expenses.Include(e => e.Category)
            .Where(e => e.UserId == userId && !e.IsDeleted);
        if (from.HasValue) query = query.Where(e => e.Date >= from.Value);
        if (to.HasValue) query = query.Where(e => e.Date <= to.Value);
        return await query.ToListAsync();
    }

    public Task<List<Expense>> GetDeletedExpensesAsync(int userId) =>
        _db.Expenses.Include(e => e.Category)
            .Where(e => e.UserId == userId && e.IsDeleted)
            .OrderByDescending(e => e.DeletedAt).ToListAsync();

    public Task<List<Expense>> GetAllExpensesAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _db.Expenses.Where(e => !e.IsDeleted);
        if (from.HasValue) query = query.Where(e => e.Date >= from.Value);
        if (to.HasValue) query = query.Where(e => e.Date <= to.Value);
        return query.ToListAsync();
    }

    public async Task AddAsync(Expense expense) => await _db.Expenses.AddAsync(expense);
    public Task UpdateAsync(Expense expense) { _db.Expenses.Update(expense); return Task.CompletedTask; }

    public Task DeleteAsync(Expense expense)
    {
        expense.IsDeleted = true;
        expense.DeletedAt = DateTime.UtcNow;
        _db.Expenses.Update(expense);
        return Task.CompletedTask;
    }

    public Task RestoreAsync(Expense expense)
    {
        expense.IsDeleted = false;
        expense.DeletedAt = null;
        expense.UpdatedAt = DateTime.UtcNow;
        _db.Expenses.Update(expense);
        return Task.CompletedTask;
    }

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

    public Task<int> GetActiveBudgetsCountAsync()
    {
        var now = DateTime.UtcNow;
        return _db.Budgets.CountAsync(b => b.Month == now.Month && b.Year == now.Year);
    }

    public async Task AddAsync(Budget budget) => await _db.Budgets.AddAsync(budget);
    public Task UpdateAsync(Budget budget) { _db.Budgets.Update(budget); return Task.CompletedTask; }
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

    public Task<List<Alert>> GetAllAsync(int userId) =>
        _db.Alerts.Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt).ToListAsync();

    public async Task<(List<Alert> Items, int Total, int Unread)> GetPagedAsync(int userId, int page, int pageSize)
    {
        var query = _db.Alerts.Where(a => a.UserId == userId).OrderByDescending(a => a.CreatedAt);
        var total = await query.CountAsync();
        var unread = await _db.Alerts.CountAsync(a => a.UserId == userId && !a.IsRead);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total, unread);
    }

    public Task<Alert?> GetByIdAsync(int id) => _db.Alerts.FindAsync(id).AsTask();

    public Task<bool> ExistsAsync(int userId, string type, int month, int year, int? budgetId = null) =>
        _db.Alerts.AnyAsync(a =>
            a.UserId == userId && a.Type == type &&
            a.Month == month && a.Year == year &&
            a.BudgetId == budgetId);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;
    public CategoryRepository(AppDbContext db) => _db = db;

    public Task<List<Category>> GetAllAsync() => _db.Categories.ToListAsync();
    public Task<Category?> GetByIdAsync(int id) => _db.Categories.FindAsync(id).AsTask();
    public Task<int> GetCountAsync() => _db.Categories.CountAsync();
    public async Task AddAsync(Category category) => await _db.Categories.AddAsync(category);
    public Task UpdateAsync(Category category) { _db.Categories.Update(category); return Task.CompletedTask; }
    public Task DeleteAsync(Category category) { _db.Categories.Remove(category); return Task.CompletedTask; }
    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

public class RevokedTokenRepository : IRevokedTokenRepository
{
    private readonly AppDbContext _db;
    public RevokedTokenRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(RevokedToken token) => await _db.RevokedTokens.AddAsync(token);

    public Task<bool> IsRevokedAsync(string token) =>
        _db.RevokedTokens.AnyAsync(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow);

    public async Task CleanupExpiredAsync()
    {
        var expired = await _db.RevokedTokens.Where(t => t.ExpiresAt <= DateTime.UtcNow).ToListAsync();
        _db.RevokedTokens.RemoveRange(expired);
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

public class RecurringExpenseRepository : IRecurringExpenseRepository
{
    private readonly AppDbContext _db;
    public RecurringExpenseRepository(AppDbContext db) => _db = db;

    public Task<List<RecurringExpense>> GetUserRecurringExpensesAsync(int userId) =>
        _db.RecurringExpenses.Include(r => r.Category)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();

    public Task<List<RecurringExpense>> GetDueRecurringExpensesAsync()
    {
        var today = DateTime.UtcNow;
        return _db.RecurringExpenses.Include(r => r.Category)
            .Where(r => r.IsActive &&
                r.DayOfMonth == today.Day &&
                (r.LastProcessedDate == null ||
                 r.LastProcessedDate.Value.Month != today.Month ||
                 r.LastProcessedDate.Value.Year != today.Year))
            .ToListAsync();
    }

    public Task<RecurringExpense?> GetByIdAsync(int id) =>
        _db.RecurringExpenses.Include(r => r.Category).FirstOrDefaultAsync(r => r.Id == id);

    public async Task AddAsync(RecurringExpense r) => await _db.RecurringExpenses.AddAsync(r);
    public Task UpdateAsync(RecurringExpense r) { _db.RecurringExpenses.Update(r); return Task.CompletedTask; }
    public Task DeleteAsync(RecurringExpense r) { _db.RecurringExpenses.Remove(r); return Task.CompletedTask; }
    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
