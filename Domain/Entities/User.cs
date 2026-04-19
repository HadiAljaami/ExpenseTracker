namespace Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string Currency { get; set; } = "SAR";

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<RecurringExpense> RecurringExpenses { get; set; } = new List<RecurringExpense>();
}
