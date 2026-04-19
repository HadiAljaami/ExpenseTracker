using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();
    public DbSet<RecurringExpense> RecurringExpenses => Set<RecurringExpense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.FullName).HasMaxLength(100);
            e.Property(u => u.Email).HasMaxLength(150);
            e.Property(u => u.Currency).HasMaxLength(10).HasDefaultValue("SAR");
        });

        modelBuilder.Entity<Expense>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasOne(x => x.User).WithMany(u => u.Expenses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category).WithMany(c => c.Expenses).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.UserId, x.IsDeleted, x.Date });
        });

        modelBuilder.Entity<Budget>(e =>
        {
            e.Property(x => x.MonthlyLimit).HasPrecision(18, 2);
            e.HasOne(x => x.User).WithMany(u => u.Budgets).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category).WithMany(c => c.Budgets).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        modelBuilder.Entity<Alert>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Message).HasMaxLength(1000);
            e.Property(x => x.Type).HasMaxLength(50);
            e.HasOne(x => x.User).WithMany(u => u.Alerts).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.UserId, x.Type, x.Month, x.Year, x.BudgetId }).HasDatabaseName("IX_Alert_Dedup");
        });

        modelBuilder.Entity<RevokedToken>(e =>
        {
            e.Property(x => x.Token).HasMaxLength(2000);
            e.HasIndex(x => x.Token);
        });

        modelBuilder.Entity<RecurringExpense>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Frequency).HasMaxLength(20);
            e.HasOne(x => x.User).WithMany(u => u.RecurringExpenses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        // Seed categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Food & Dining", Icon = "🍔", Color = "#FF6B6B", CreatedAt = DateTime.UtcNow },
            new Category { Id = 2, Name = "Transport", Icon = "🚗", Color = "#4ECDC4", CreatedAt = DateTime.UtcNow },
            new Category { Id = 3, Name = "Bills & Utilities", Icon = "💡", Color = "#45B7D1", CreatedAt = DateTime.UtcNow },
            new Category { Id = 4, Name = "Entertainment", Icon = "🎬", Color = "#96CEB4", CreatedAt = DateTime.UtcNow },
            new Category { Id = 5, Name = "Healthcare", Icon = "🏥", Color = "#FFEAA7", CreatedAt = DateTime.UtcNow },
            new Category { Id = 6, Name = "Shopping", Icon = "🛍️", Color = "#DDA0DD", CreatedAt = DateTime.UtcNow },
            new Category { Id = 7, Name = "Education", Icon = "📚", Color = "#98D8C8", CreatedAt = DateTime.UtcNow },
            new Category { Id = 8, Name = "Other", Icon = "📦", Color = "#B0BEC5", CreatedAt = DateTime.UtcNow }
        );
    }
}
