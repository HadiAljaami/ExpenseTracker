using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public static class DataSeeder
{
    /// <summary>
    /// Seeds the first Admin user if no admin exists.
    /// Default credentials: admin@expensetracker.com / Admin@123456
    /// CHANGE THESE IN PRODUCTION via environment variables!
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await db.Database.MigrateAsync();

        // Only seed if no admin exists
        var adminExists = await db.Users.AnyAsync(u => u.Role == UserRoles.Admin);
        if (adminExists) return;

        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@expensetracker.com";
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123456";
        var adminName = Environment.GetEnvironmentVariable("ADMIN_NAME") ?? "System Admin";

        var admin = new User
        {
            FullName = adminName,
            Email = adminEmail.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = UserRoles.Admin,
            Currency = "SAR",
            IsSuspended = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        logger.LogInformation("✅ Default admin created: {Email}", adminEmail);
        logger.LogWarning("⚠️  Change the default admin password immediately in production!");
    }
}
