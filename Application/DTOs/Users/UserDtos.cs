namespace Application.DTOs.Users;

public class UserProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Currency { get; set; } = "SAR";
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

// ─── Admin DTOs ───────────────────────────────────────────

public class AdminUserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public bool IsSuspended { get; set; }
    public int TotalExpenses { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUserFilterDto
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsSuspended { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PagedAdminUsersDto
{
    public List<AdminUserDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class ChangeUserRoleDto
{
    public string Role { get; set; } = string.Empty;
}

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalExpenses { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalCategories { get; set; }
    public int ActiveBudgets { get; set; }
    public int NewUsersThisMonth { get; set; }
    public decimal ExpensesThisMonth { get; set; }
    public decimal ExpensesLastMonth { get; set; }
    public double GrowthPercentage => ExpensesLastMonth > 0
        ? Math.Round((double)((ExpensesThisMonth - ExpensesLastMonth) / ExpensesLastMonth) * 100, 2)
        : 0;
    public List<TopSpenderDto> TopSpenders { get; set; } = new();
    public List<SystemMonthlyDto> MonthlyGrowth { get; set; } = new();
    public List<SystemCategoryDto> TopCategories { get; set; } = new();
}

public class TopSpenderDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ExpenseCount { get; set; }
}

public class SystemMonthlyDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public int NewUsers { get; set; }
}

public class SystemCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int UsageCount { get; set; }
    public double Percentage { get; set; }
}
