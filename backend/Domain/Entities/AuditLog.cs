namespace Domain.Entities;

public class AuditLog : BaseEntity
{
    public int AdminId { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;       // e.g. "DeleteUser", "ChangeRole"
    public string EntityType { get; set; } = string.Empty;   // e.g. "User", "Category"
    public int? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }

    public User Admin { get; set; } = null!;
}
