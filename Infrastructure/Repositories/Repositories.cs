using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

// ─── User Repository ──────────────────────────────────────────────────────────
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

    public async Task<(List<User> Items, int Total)> GetPagedAsync(
        string? search, string? role, bool? isSuspended,
        string sortBy, string sortDir, int page, int pageSize)
    {
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);

        if (isSuspended.HasValue)
            query = query.Where(u => u.IsSuspended == isSuspended.Value);

        query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
        {
            ("fullname", "asc") => query.OrderBy(u => u.FullName),
            ("fullname", _)     => query.OrderByDescending(u => u.FullName),
            ("email", "asc")    => query.OrderBy(u => u.Email),
            ("email", _)        => query.OrderByDescending(u => u.Email),
            (_, "asc")          => query.OrderBy(u => u.CreatedAt),
            _                   => query.OrderByDescending(u => u.CreatedAt)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    // ✅ حل N+1: نجلب إحصائيات كل المستخدمين في query واحدة
    public async Task<Dictionary<int, (int Count, decimal Total)>> GetUsersExpenseStatsAsync(List<int> userIds)
    {
        var stats = await _db.Expenses
            .Where(e => userIds.Contains(e.UserId) && !e.IsDeleted)
            .GroupBy(e => e.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count(), Total = g.Sum(e => e.Amount) })
            .ToListAsync();

        return stats.ToDictionary(s => s.UserId, s => (s.Count, s.Total));
    }

    public Task<int> GetCountByMonthAsync(int month, int year) =>
        _db.Users.CountAsync(u => u.CreatedAt.Month == month && u.CreatedAt.Year == year);

    public async Task AddAsync(User user) => await _db.Users.AddAsync(user);
    public Task DeleteAsync(User user) { _db.Users.Remove(user); return Task.CompletedTask; }
    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

// ─── Expense Repository ───────────────────────────────────────────────────────
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
        if (to.HasValue)   query = query.Where(e => e.Date <= to.Value);
        return await query.ToListAsync();
    }

    // ✅ حل Memory Filtering: كل الـ Filtering يتم في قاعدة البيانات
    public async Task<(List<Expense> Items, int Total)> GetFilteredAsync(
        int userId, int? categoryId, DateTime? from, DateTime? to,
        decimal? minAmount, decimal? maxAmount, string? search,
        string sortBy, string sortDir, int page, int pageSize)
    {
        var query = _db.Expenses.Include(e => e.Category)
            .Where(e => e.UserId == userId && !e.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        if (from.HasValue)
            query = query.Where(e => e.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.Date <= to.Value);

        if (minAmount.HasValue)
            query = query.Where(e => e.Amount >= minAmount.Value);

        if (maxAmount.HasValue)
            query = query.Where(e => e.Amount <= maxAmount.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Description.Contains(search));

        query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
        {
            ("amount", "asc")  => query.OrderBy(e => e.Amount),
            ("amount", _)      => query.OrderByDescending(e => e.Amount),
            ("category", "asc") => query.OrderBy(e => e.Category.Name),
            ("category", _)    => query.OrderByDescending(e => e.Category.Name),
            (_, "asc")         => query.OrderBy(e => e.Date),
            _                  => query.OrderByDescending(e => e.Date)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    // ✅ Admin: Filter all expenses across all users in DB
    public async Task<(List<Expense> Items, int Total, decimal TotalAmount)> GetAllFilteredAsync(
        int? userId, int? categoryId, DateTime? from, DateTime? to,
        string? search, string sortBy, string sortDir, int page, int pageSize)
    {
        var query = _db.Expenses.Include(e => e.Category).Where(e => !e.IsDeleted);

        if (userId.HasValue) query = query.Where(e => e.UserId == userId.Value);
        if (categoryId.HasValue) query = query.Where(e => e.CategoryId == categoryId.Value);
        if (from.HasValue) query = query.Where(e => e.Date >= from.Value);
        if (to.HasValue) query = query.Where(e => e.Date <= to.Value);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(e => e.Description.Contains(search));

        query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
        {
            ("amount", "asc")  => query.OrderBy(e => e.Amount),
            ("amount", _)      => query.OrderByDescending(e => e.Amount),
            (_, "asc")         => query.OrderBy(e => e.Date),
            _                  => query.OrderByDescending(e => e.Date)
        };

        var total = await query.CountAsync();
        var totalAmount = await query.SumAsync(e => e.Amount);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total, totalAmount);
    }

    public Task<List<Expense>> GetDeletedExpensesAsync(int userId) =>
        _db.Expenses.Include(e => e.Category)
            .Where(e => e.UserId == userId && e.IsDeleted)
            .OrderByDescending(e => e.DeletedAt).ToListAsync();

    public Task<List<Expense>> GetAllExpensesAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _db.Expenses.Include(e => e.Category).Where(e => !e.IsDeleted);
        if (from.HasValue) query = query.Where(e => e.Date >= from.Value);
        if (to.HasValue)   query = query.Where(e => e.Date <= to.Value);
        return query.ToListAsync();
    }

    // ✅ حل N+1 في Admin: query واحدة لكل المستخدمين
    public async Task<Dictionary<int, (int Count, decimal Total)>> GetExpenseStatsByUserIdsAsync(List<int> userIds)
    {
        var stats = await _db.Expenses
            .Where(e => userIds.Contains(e.UserId) && !e.IsDeleted)
            .GroupBy(e => e.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count(), Total = g.Sum(e => e.Amount) })
            .ToListAsync();

        return stats.ToDictionary(s => s.UserId, s => (s.Count, s.Total));
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

// ─── Budget Repository ────────────────────────────────────────────────────────
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

// ─── Alert Repository ─────────────────────────────────────────────────────────
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
        var total  = await query.CountAsync();
        var unread = await _db.Alerts.CountAsync(a => a.UserId == userId && !a.IsRead);
        var items  = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
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

// ─── Category Repository ──────────────────────────────────────────────────────
public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;
    public CategoryRepository(AppDbContext db) => _db = db;

    public Task<List<Category>> GetAllAsync() => _db.Categories.ToListAsync();
    public Task<Category?> GetByIdAsync(int id) => _db.Categories.FindAsync(id).AsTask();
    public Task<int> GetCountAsync() => _db.Categories.CountAsync();
    public async Task AddAsync(Category c) => await _db.Categories.AddAsync(c);
    public Task UpdateAsync(Category c) { _db.Categories.Update(c); return Task.CompletedTask; }
    public Task DeleteAsync(Category c) { _db.Categories.Remove(c); return Task.CompletedTask; }
    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

// ─── RevokedToken Repository ──────────────────────────────────────────────────
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

// ─── RefreshToken Repository ──────────────────────────────────────────────────
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;
    public RefreshTokenRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(RefreshToken token) => await _db.RefreshTokens.AddAsync(token);

    public Task<RefreshToken?> GetByTokenAsync(string token) =>
        _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);

    public Task<List<RefreshToken>> GetUserTokensAsync(int userId) =>
        _db.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

// ─── PasswordReset Repository ─────────────────────────────────────────────────
public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly AppDbContext _db;
    public PasswordResetRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(PasswordResetToken token) => await _db.PasswordResetTokens.AddAsync(token);

    public Task<PasswordResetToken?> GetByTokenAsync(string token) =>
        _db.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed);

    public Task DeleteAsync(PasswordResetToken token)
    {
        _db.PasswordResetTokens.Remove(token);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

// ─── RecurringExpense Repository ─────────────────────────────────────────────
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

// ─── AuditLog Repository ──────────────────────────────────────────────────────
public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _db;
    public AuditLogRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(AuditLog log) => await _db.AuditLogs.AddAsync(log);

    public async Task<(List<AuditLog> Items, int Total)> GetPagedAsync(
        int page, int pageSize, int? adminId = null, string? action = null)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (adminId.HasValue)
            query = query.Where(a => a.AdminId == adminId.Value);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        query = query.OrderByDescending(a => a.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
