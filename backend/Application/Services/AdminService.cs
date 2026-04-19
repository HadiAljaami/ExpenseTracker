using Application.DTOs.Admin;
using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepo;
    private readonly IExpenseRepository _expenseRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IBudgetRepository _budgetRepo;
    private readonly IAuditLogRepository _auditRepo;

    public AdminService(IUserRepository userRepo, IExpenseRepository expenseRepo,
        ICategoryRepository categoryRepo, IBudgetRepository budgetRepo,
        IAuditLogRepository auditRepo)
    {
        _userRepo = userRepo;
        _expenseRepo = expenseRepo;
        _categoryRepo = categoryRepo;
        _budgetRepo = budgetRepo;
        _auditRepo = auditRepo;
    }

    // ─── Users ────────────────────────────────────────────────────────────────

    public async Task<PagedAdminUsersDto> GetAllUsersAsync(AdminUserFilterDto filter)
    {
        var (users, total) = await _userRepo.GetPagedAsync(
            filter.Search, filter.Role, filter.IsSuspended,
            filter.SortBy ?? "CreatedAt", filter.SortDirection ?? "desc",
            filter.Page, filter.PageSize);

        var userIds = users.Select(u => u.Id).ToList();
        var stats = await _expenseRepo.GetExpenseStatsByUserIdsAsync(userIds);

        var items = users.Select(user =>
        {
            stats.TryGetValue(user.Id, out var s);
            return MapToAdminDto(user, s.Count, s.Total);
        }).ToList();

        return new PagedAdminUsersDto
        {
            Items = items, TotalCount = total,
            Page = filter.Page, PageSize = filter.PageSize
        };
    }

    public async Task<AdminUserDto> GetUserByIdAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);
        var stats = await _expenseRepo.GetExpenseStatsByUserIdsAsync(new List<int> { userId });
        stats.TryGetValue(userId, out var s);
        return MapToAdminDto(user, s.Count, s.Total);
    }

    public async Task<AdminUserDto> CreateUserAsync(CreateAdminUserDto dto)
    {
        var existing = await _userRepo.GetByEmailAsync(dto.Email.ToLower());
        if (existing != null)
            throw new ConflictException("Email already registered.");

        if (dto.Role != UserRoles.User && dto.Role != UserRoles.Admin)
            throw new BusinessException($"Role must be '{UserRoles.User}' or '{UserRoles.Admin}'.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            Currency = "SAR"
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();
        return MapToAdminDto(user, 0, 0);
    }

    public async Task DeleteUserAsync(int adminId, string adminEmail, int userId, string? ipAddress)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (user.Role == UserRoles.Admin)
            throw new BusinessException("Cannot delete an admin user.");

        await _auditRepo.AddAsync(new AuditLog
        {
            AdminId = adminId, AdminEmail = adminEmail,
            Action = AdminActions.DeleteUser, EntityType = "User", EntityId = userId,
            OldValue = $"{user.FullName} ({user.Email})", IpAddress = ipAddress
        });

        await _userRepo.DeleteAsync(user);
        await _userRepo.SaveChangesAsync();
        await _auditRepo.SaveChangesAsync();
    }

    public async Task<AdminUserDto> ChangeUserRoleAsync(int adminId, string adminEmail, int userId, string role, string? ipAddress)
    {
        if (role != UserRoles.User && role != UserRoles.Admin)
            throw new BusinessException($"Role must be '{UserRoles.User}' or '{UserRoles.Admin}'.");

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var oldRole = user.Role;
        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;

        await _auditRepo.AddAsync(new AuditLog
        {
            AdminId = adminId, AdminEmail = adminEmail,
            Action = AdminActions.ChangeRole, EntityType = "User", EntityId = userId,
            OldValue = oldRole, NewValue = role, IpAddress = ipAddress
        });

        await _userRepo.SaveChangesAsync();
        await _auditRepo.SaveChangesAsync();

        var stats = await _expenseRepo.GetExpenseStatsByUserIdsAsync(new List<int> { userId });
        stats.TryGetValue(userId, out var s);
        return MapToAdminDto(user, s.Count, s.Total);
    }

    public async Task<AdminUserDto> ToggleSuspendUserAsync(int adminId, string adminEmail, int userId, string? ipAddress)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (user.Role == UserRoles.Admin)
            throw new BusinessException("Cannot suspend an admin user.");

        user.IsSuspended = !user.IsSuspended;
        user.UpdatedAt = DateTime.UtcNow;

        await _auditRepo.AddAsync(new AuditLog
        {
            AdminId = adminId, AdminEmail = adminEmail,
            Action = user.IsSuspended ? AdminActions.SuspendUser : AdminActions.UnsuspendUser,
            EntityType = "User", EntityId = userId,
            NewValue = user.IsSuspended ? "Suspended" : "Active", IpAddress = ipAddress
        });

        await _userRepo.SaveChangesAsync();
        await _auditRepo.SaveChangesAsync();

        var stats = await _expenseRepo.GetExpenseStatsByUserIdsAsync(new List<int> { userId });
        stats.TryGetValue(userId, out var s);
        return MapToAdminDto(user, s.Count, s.Total);
    }

    // ─── Expenses ─────────────────────────────────────────────────────────────

    public async Task<PagedAdminExpensesDto> GetAllExpensesAsync(AdminExpenseFilterDto filter)
    {
        var (expenses, total, totalAmount) = await _expenseRepo.GetAllFilteredAsync(
            filter.UserId, filter.CategoryId,
            filter.FromDate, filter.ToDate, filter.Search,
            filter.SortBy ?? "Date", filter.SortDirection ?? "desc",
            filter.Page, filter.PageSize);

        var userIds = expenses.Select(e => e.UserId).Distinct().ToList();
        var users = await _userRepo.GetAllAsync();
        var userMap = users.ToDictionary(u => u.Id, u => u);

        var items = expenses.Select(e =>
        {
            userMap.TryGetValue(e.UserId, out var user);
            return new AdminExpenseDto
            {
                Id = e.Id,
                UserFullName = user?.FullName ?? "Unknown",
                UserEmail = user?.Email ?? "Unknown",
                Amount = e.Amount,
                Description = e.Description,
                Date = e.Date,
                CategoryName = e.Category.Name,
                CategoryIcon = e.Category.Icon
            };
        }).ToList();

        return new PagedAdminExpensesDto
        {
            Items = items, TotalCount = total, TotalAmount = totalAmount,
            Page = filter.Page, PageSize = filter.PageSize
        };
    }

    public async Task DeleteExpenseAsync(int adminId, string adminEmail, int expenseId, string? ipAddress)
    {
        var expense = await _expenseRepo.GetByIdAsync(expenseId)
            ?? throw new NotFoundException("Expense", expenseId);

        await _auditRepo.AddAsync(new AuditLog
        {
            AdminId = adminId, AdminEmail = adminEmail,
            Action = AdminActions.DeleteExpense, EntityType = "Expense", EntityId = expenseId,
            OldValue = $"Amount: {expense.Amount}, User: {expense.UserId}", IpAddress = ipAddress
        });

        await _expenseRepo.DeleteAsync(expense);
        await _expenseRepo.SaveChangesAsync();
        await _auditRepo.SaveChangesAsync();
    }

    // ─── Reports ──────────────────────────────────────────────────────────────

    public async Task<AdminStatsDto> GetSystemStatsAsync()
    {
        var now = DateTime.UtcNow;
        var users = await _userRepo.GetAllAsync();
        var allExpenses = await _expenseRepo.GetAllExpensesAsync();
        var categoryCount = await _categoryRepo.GetCountAsync();
        var activeBudgets = await _budgetRepo.GetActiveBudgetsCountAsync();

        var thisMonth = allExpenses.Where(e => e.Date.Month == now.Month && e.Date.Year == now.Year).ToList();
        var lastMonth = allExpenses.Where(e => e.Date.Month == now.AddMonths(-1).Month && e.Date.Year == now.AddMonths(-1).Year).ToList();
        var newUsersThisMonth = await _userRepo.GetCountByMonthAsync(now.Month, now.Year);

        var userIds = users.Select(u => u.Id).ToList();
        var userStats = await _expenseRepo.GetExpenseStatsByUserIdsAsync(userIds);

        var topSpenders = users
            .Select(u => { userStats.TryGetValue(u.Id, out var s); return (User: u, Stats: s); })
            .Where(x => x.Stats.Count > 0)
            .OrderByDescending(x => x.Stats.Total)
            .Take(5)
            .Select(x => new TopSpenderDto
            {
                FullName = x.User.FullName, Email = x.User.Email,
                TotalAmount = x.Stats.Total, ExpenseCount = x.Stats.Count
            }).ToList();

        var totalAmount = allExpenses.Sum(e => e.Amount);
        var topCategories = allExpenses
            .GroupBy(e => e.Category)
            .Select(g => new SystemCategoryDto
            {
                CategoryName = g.Key.Name, CategoryIcon = g.Key.Icon,
                TotalAmount = g.Sum(e => e.Amount), UsageCount = g.Count(),
                Percentage = totalAmount > 0 ? Math.Round((double)(g.Sum(e => e.Amount) / totalAmount) * 100, 2) : 0
            })
            .OrderByDescending(c => c.TotalAmount).Take(5).ToList();

        return new AdminStatsDto
        {
            TotalUsers = users.Count,
            TotalExpenses = allExpenses.Count,
            TotalAmount = totalAmount,
            TotalCategories = categoryCount,
            ActiveBudgets = activeBudgets,
            NewUsersThisMonth = newUsersThisMonth,
            ExpensesThisMonth = thisMonth.Sum(e => e.Amount),
            ExpensesLastMonth = lastMonth.Sum(e => e.Amount),
            TopSpenders = topSpenders,
            MonthlyGrowth = await GetMonthlyGrowthAsync(now.Year),
            TopCategories = topCategories
        };
    }

    public async Task<List<SystemMonthlyDto>> GetMonthlyGrowthAsync(int year)
    {
        var allExpenses = await _expenseRepo.GetAllExpensesAsync(
            new DateTime(year, 1, 1), new DateTime(year, 12, 31));

        var result = new List<SystemMonthlyDto>();
        for (int m = 1; m <= 12; m++)
        {
            var monthExpenses = allExpenses.Where(e => e.Date.Month == m).ToList();
            var newUsers = await _userRepo.GetCountByMonthAsync(m, year);
            result.Add(new SystemMonthlyDto
            {
                Month = m, MonthName = new DateTime(year, m, 1).ToString("MMM"), Year = year,
                TotalAmount = monthExpenses.Sum(e => e.Amount),
                TransactionCount = monthExpenses.Count, NewUsers = newUsers
            });
        }
        return result;
    }

    public async Task<AdminSystemReportDto> GetSystemReportAsync(int month, int year)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var allExpenses = await _expenseRepo.GetAllExpensesAsync(from, to);
        var users = await _userRepo.GetAllAsync();
        var categoryCount = await _categoryRepo.GetCountAsync();
        var activeBudgets = await _budgetRepo.GetActiveBudgetsCountAsync();
        var newUsers = await _userRepo.GetCountByMonthAsync(month, year);
        var suspendedUsers = users.Count(u => u.IsSuspended);
        var totalAmount = allExpenses.Sum(e => e.Amount);

        var categoryBreakdown = allExpenses
            .GroupBy(e => e.Category)
            .Select(g => new SystemCategoryBreakdownDto
            {
                CategoryName = g.Key.Name, CategoryIcon = g.Key.Icon,
                TotalAmount = g.Sum(e => e.Amount), TransactionCount = g.Count(),
                Percentage = totalAmount > 0 ? Math.Round((double)(g.Sum(e => e.Amount) / totalAmount) * 100, 2) : 0
            })
            .OrderByDescending(c => c.TotalAmount).ToList();

        var dailyBreakdown = allExpenses
            .GroupBy(e => e.Date.Date)
            .Select(g => new DailySystemDto
            {
                Date = g.Key, Amount = g.Sum(e => e.Amount), TransactionCount = g.Count()
            })
            .OrderBy(d => d.Date).ToList();

        return new AdminSystemReportDto
        {
            Year = year, Month = month,
            MonthName = new DateTime(year, month, 1).ToString("MMMM yyyy"),
            TotalUsers = users.Count, NewUsersThisMonth = newUsers,
            TotalExpenses = allExpenses.Count, TotalAmount = totalAmount,
            SuspendedUsers = suspendedUsers, TotalCategories = categoryCount,
            ActiveBudgets = activeBudgets,
            CategoryBreakdown = categoryBreakdown, DailyBreakdown = dailyBreakdown
        };
    }

    // ─── Audit Log ────────────────────────────────────────────────────────────

    public async Task<PagedAuditLogsDto> GetAuditLogsAsync(int page, int pageSize, int? adminId, string? action)
    {
        var (items, total) = await _auditRepo.GetPagedAsync(page, pageSize, adminId, action);
        return new PagedAuditLogsDto
        {
            Items = items.Select(a => new AuditLogDto
            {
                Id = a.Id, AdminEmail = a.AdminEmail, Action = a.Action,
                EntityType = a.EntityType, EntityId = a.EntityId,
                OldValue = a.OldValue, NewValue = a.NewValue,
                IpAddress = a.IpAddress, CreatedAt = a.CreatedAt
            }).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    private static AdminUserDto MapToAdminDto(User user, int expenseCount, decimal totalAmount) => new()
    {
        Id = user.Id, FullName = user.FullName, Email = user.Email,
        Role = user.Role, Currency = user.Currency, IsSuspended = user.IsSuspended,
        TotalExpenses = expenseCount, TotalAmount = totalAmount, CreatedAt = user.CreatedAt
    };
}
