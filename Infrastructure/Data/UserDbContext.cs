using Microsoft.EntityFrameworkCore;
using UserReportService.Domain.Entities;
using UserReportService.Domain.Enums;

namespace UserReportService.Infrastructure.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<DailyRevenue> DailyRevenues => Set<DailyRevenue>();
    public DbSet<ProductSold> ProductsSold => Set<ProductSold>();
    public DbSet<CustomerSpent> CustomersSpent => Set<CustomerSpent>();
    public DbSet<OrderReceipt> OrderReceipts => Set<OrderReceipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
            entity.Property(e => e.Role)
                .HasConversion(
                    v => v.ToString(),
                    v => (UserRole)Enum.Parse(typeof(UserRole), v))
                .HasMaxLength(50);
        });

        // Daily Revenue Configuration
        modelBuilder.Entity<DailyRevenue>(entity =>
        {
            entity.HasKey(e => e.Date);
            entity.Property(e => e.TotalRevenue).HasPrecision(18, 2);
        });

        // Product Sold Configuration
        modelBuilder.Entity<ProductSold>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.Date });
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(250);
            entity.Property(e => e.TotalRevenue).HasPrecision(18, 2);
        });

        // Customer Spent Configuration
        modelBuilder.Entity<CustomerSpent>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.CustomerId).ValueGeneratedNever(); // Supplied externally
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(250);
            entity.Property(e => e.TotalSpent).HasPrecision(18, 2);
        });

        // Order Receipt Configuration
        modelBuilder.Entity<OrderReceipt>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
        });
    }
}
