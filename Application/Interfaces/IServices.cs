using Application.DTOs;
using Application.DTOs.Auth;
using Application.DTOs.Expenses;
using Application.DTOs.Budgets;
using Application.DTOs.Insights;
using Application.DTOs.Reports;
namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}

public interface IExpenseService
{
    Task<ExpenseResponseDto> CreateAsync(int userId, CreateExpenseDto dto);
    Task<ExpenseResponseDto> UpdateAsync(int userId, int expenseId, UpdateExpenseDto dto);
    Task DeleteAsync(int userId, int expenseId);
    Task<ExpenseResponseDto> GetByIdAsync(int userId, int expenseId);
    Task<PagedExpensesDto> GetAllAsync(int userId, ExpenseFilterDto filter);
}

public interface IBudgetService
{
    Task<BudgetResponseDto> CreateAsync(int userId, CreateBudgetDto dto);
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
}

public interface IAlertService
{
    Task CheckAndCreateAlertsAsync(int userId);
    Task<List<AlertDto>> GetUnreadAlertsAsync(int userId);
    Task MarkAsReadAsync(int userId, int alertId);
}
