using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task<(List<User> Items, int Total)> GetPagedAsync(string? search, string? role, bool? isSuspended, string sortBy, string sortDir, int page, int pageSize);
    Task<Dictionary<int, (int Count, decimal Total)>> GetUsersExpenseStatsAsync(List<int> userIds);
    Task<int> GetCountByMonthAsync(int month, int year);
    Task AddAsync(User user);
    Task DeleteAsync(User user);
    Task SaveChangesAsync();
}

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(int id);
    Task<Expense?> GetByIdIncludeDeletedAsync(int id);
    Task<List<Expense>> GetUserExpensesAsync(int userId, DateTime? from = null, DateTime? to = null);
    Task<(List<Expense> Items, int Total)> GetFilteredAsync(int userId, int? categoryId, DateTime? from, DateTime? to, decimal? minAmount, decimal? maxAmount, string? search, string sortBy, string sortDir, int page, int pageSize);
    Task<List<Expense>> GetDeletedExpensesAsync(int userId);
    Task<List<Expense>> GetAllExpensesAsync(DateTime? from = null, DateTime? to = null);
    Task<(List<Expense> Items, int Total, decimal TotalAmount)> GetAllFilteredAsync(int? userId, int? categoryId, DateTime? from, DateTime? to, string? search, string sortBy, string sortDir, int page, int pageSize);
    Task<Dictionary<int, (int Count, decimal Total)>> GetExpenseStatsByUserIdsAsync(List<int> userIds);
    Task AddAsync(Expense expense);
    Task UpdateAsync(Expense expense);
    Task DeleteAsync(Expense expense);
    Task RestoreAsync(Expense expense);
    Task SaveChangesAsync();
}

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(int id);
    Task<List<Budget>> GetUserBudgetsAsync(int userId, int month, int year);
    Task<int> GetActiveBudgetsCountAsync();
    Task AddAsync(Budget budget);
    Task UpdateAsync(Budget budget);
    Task DeleteAsync(Budget budget);
    Task SaveChangesAsync();
}

public interface IAlertRepository
{
    Task AddAsync(Alert alert);
    Task<List<Alert>> GetUnreadAsync(int userId);
    Task<List<Alert>> GetAllAsync(int userId);
    Task<(List<Alert> Items, int Total, int Unread)> GetPagedAsync(int userId, int page, int pageSize);
    Task<Alert?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int userId, string type, int month, int year, int? budgetId = null);
    Task SaveChangesAsync();
}

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<int> GetCountAsync();
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
    Task SaveChangesAsync();
}

public interface IRevokedTokenRepository
{
    Task AddAsync(RevokedToken token);
    Task<bool> IsRevokedAsync(string token);
    Task CleanupExpiredAsync();
    Task SaveChangesAsync();
}

public interface IRecurringExpenseRepository
{
    Task<List<RecurringExpense>> GetUserRecurringExpensesAsync(int userId);
    Task<List<RecurringExpense>> GetDueRecurringExpensesAsync();
    Task<RecurringExpense?> GetByIdAsync(int id);
    Task AddAsync(RecurringExpense recurringExpense);
    Task UpdateAsync(RecurringExpense recurringExpense);
    Task DeleteAsync(RecurringExpense recurringExpense);
    Task SaveChangesAsync();
}

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<List<RefreshToken>> GetUserTokensAsync(int userId);
    Task SaveChangesAsync();
}

public interface IPasswordResetRepository
{
    Task AddAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task DeleteAsync(PasswordResetToken token);
    Task SaveChangesAsync();
}

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<(List<AuditLog> Items, int Total)> GetPagedAsync(int page, int pageSize, int? adminId = null, string? action = null);
    Task SaveChangesAsync();
}
