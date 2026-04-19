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

// Admin DTOs
public class AdminUserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public int TotalExpenses { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalExpenses { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalCategories { get; set; }
    public int ActiveBudgets { get; set; }
    public List<TopSpenderDto> TopSpenders { get; set; } = new();
}

public class TopSpenderDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ExpenseCount { get; set; }
}

