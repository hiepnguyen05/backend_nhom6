using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserReportService.Domain.Entities;
using UserReportService.Domain.Enums;

namespace UserReportService.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var context = serviceProvider.GetRequiredService<UserDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<UserDbContext>>();

        try
        {
            logger.LogInformation("Applying migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");

            // Seed Admin User
            if (!await context.Users.AnyAsync())
            {
                logger.LogInformation("Seeding default Admin user...");
                
                var adminUser = new User
                {
                    Email = "admin@shop.com",
                    FullName = "System Admin",
                    Phone = "0123456789",
                    Role = UserRole.Admin,
                    Status = UserStatus.Active,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
                logger.LogInformation("Default Admin user seeded successfully. (admin@shop.com / Admin@123)");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            throw;
        }
    }
}
