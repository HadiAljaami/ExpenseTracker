using Application.DTOs;
using Application.DTOs.Auth;
using Application.DTOs.Expenses;
using Application.DTOs.Budgets;
using Application.DTOs.Insights;
using Application.DTOs.Reports;
using Application.DTOs.Users;
using Application.DTOs.RecurringExpenses;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task LogoutAsync(string token);
}

public interface IExpenseService
{
    Task<ExpenseResponseDto> CreateAsync(int userId, CreateExpenseDto dto);
    Task<ExpenseResponseDto> UpdateAsync(int userId, int expenseId, UpdateExpenseDto dto);
    Task DeleteAsync(int userId, int expenseId);
    Task RestoreAsync(int userId, int expenseId);
    Task<ExpenseResponseDto> GetByIdAsync(int userId, int expenseId);
    Task<PagedExpensesDto> GetAllAsync(int userId, ExpenseFilterDto filter);
    Task<List<ExpenseResponseDto>> GetDeletedAsync(int userId);
}

public interface IBudgetService
{
    Task<BudgetResponseDto> CreateAsync(int userId, CreateBudgetDto dto);
    Task<BudgetResponseDto> UpdateAsync(int userId, int budgetId, CreateBudgetDto dto);
    Task<List<BudgetResponseDto>> GetAllAsync(int userId, int month, int year);
    Task DeleteAsync(int userId, int budgetId);
}

public interface IInsightsService
{
    Task<InsightsSummaryDto> GetSummaryAsync(int userId, int month, int year);
}

public interface IReportService
{
    Task<MonthlyReportDto> GetMonthlyReportAsync(int userId, int month, int year);
    Task<YearlyReportDto> GetYearlyReportAsync(int userId, int year);
}

public interface IAlertService
{
    Task CheckAndCreateAlertsAsync(int userId);
    Task<PagedAlertsDto> GetAlertsAsync(int userId, int page, int pageSize, bool? unreadOnly = null);
    Task<List<AlertDto>> GetUnreadAlertsAsync(int userId);
    Task<List<AlertDto>> GetAllAlertsAsync(int userId);
    Task MarkAsReadAsync(int userId, int alertId);
    Task MarkAllAsReadAsync(int userId);
}

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task DeleteAccountAsync(int userId);
}

public interface IAdminService
{
    Task<List<AdminUserDto>> GetAllUsersAsync();
    Task<AdminUserDto> GetUserByIdAsync(int userId);
    Task DeleteUserAsync(int userId);
    Task<AdminStatsDto> GetSystemStatsAsync();
}

public interface IRecurringExpenseService
{
    Task<RecurringExpenseResponseDto> CreateAsync(int userId, CreateRecurringExpenseDto dto);
    Task<List<RecurringExpenseResponseDto>> GetAllAsync(int userId);
    Task<RecurringExpenseResponseDto> ToggleActiveAsync(int userId, int id);
    Task DeleteAsync(int userId, int id);
    Task ProcessDueRecurringExpensesAsync();
}

public interface IExportService
{
    Task<byte[]> ExportExpensesToExcelAsync(int userId, int month, int year);
    Task<byte[]> ExportMonthlyReportToPdfAsync(int userId, int month, int year);
}
