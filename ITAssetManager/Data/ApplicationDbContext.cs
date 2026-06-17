using ITAssetManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<AssetCategory> AssetCategories { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetAssignment> AssetAssignments { get; set; }
    public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Seed: دسته‌بندی‌های پیش‌فرض
        builder.Entity<AssetCategory>().HasData(
            new AssetCategory { Id = 1, Name = "لپ‌تاپ", Icon = "bi-laptop" },
            new AssetCategory { Id = 2, Name = "کامپیوتر رومیزی", Icon = "bi-pc-display" },
            new AssetCategory { Id = 3, Name = "مانیتور", Icon = "bi-display" },
            new AssetCategory { Id = 4, Name = "پرینتر", Icon = "bi-printer" },
            new AssetCategory { Id = 5, Name = "ماوس", Icon = "bi-mouse" },
            new AssetCategory { Id = 6, Name = "کیبورد", Icon = "bi-keyboard" },
            new AssetCategory { Id = 7, Name = "سوئیچ شبکه", Icon = "bi-hdd-network" },
            new AssetCategory { Id = 8, Name = "روتر", Icon = "bi-router" },
            new AssetCategory { Id = 9, Name = "سرور", Icon = "bi-server" },
            new AssetCategory { Id = 10, Name = "UPS", Icon = "bi-battery-charging" },
            new AssetCategory { Id = 11, Name = "هدست", Icon = "bi-headset" },
            new AssetCategory { Id = 12, Name = "سایر", Icon = "bi-box" }
        );

        // تنظیم رابطه‌ها - همه ON DELETE NO ACTION تا از cascade cycle جلوگیری بشه
        builder.Entity<Asset>()
            .HasOne(a => a.Department)
            .WithMany(d => d.Assets)
            .HasForeignKey(a => a.DepartmentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Asset>()
            .HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<AssetAssignment>()
            .HasOne(a => a.FromEmployee)
            .WithMany()
            .HasForeignKey(a => a.FromEmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<AssetAssignment>()
            .HasOne(a => a.ToEmployee)
            .WithMany(e => e.AssetAssignments)
            .HasForeignKey(a => a.ToEmployeeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<AssetAssignment>()
            .HasOne(a => a.FromDepartment)
            .WithMany()
            .HasForeignKey(a => a.FromDepartmentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<AssetAssignment>()
            .HasOne(a => a.ToDepartment)
            .WithMany()
            .HasForeignKey(a => a.ToDepartmentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
