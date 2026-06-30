using ITAssetManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<AssetCategory> AssetCategories { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetAssignment> AssetAssignments { get; set; }
    public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
    public DbSet<SpecDefinition> SpecDefinitions { get; set; }
    public DbSet<SpecValue> SpecValues { get; set; }
    public DbSet<AssetSpecValue> AssetSpecValues { get; set; }
    public DbSet<VwEmployee> VwEmployees { get; set; }
    public DbSet<VwDepartment> VwDepartments { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<VwPurchaseRequest> VwPurchaseRequests { get; set; }
    public DbSet<AssemblyComponent> AssemblyComponents { get; set; }

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


        builder.Entity<SpecDefinition>()
            .HasOne(s => s.Category)
            .WithMany()
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<SpecValue>()
            .HasOne(s => s.SpecDefinition)
            .WithMany(d => d.SpecValues)
            .HasForeignKey(s => s.SpecDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AssetSpecValue>()
            .HasOne(a => a.Asset)
            .WithMany(a => a.SpecValues)
            .HasForeignKey(a => a.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AssetSpecValue>()
            .HasOne(a => a.SpecDefinition)
            .WithMany(d => d.AssetSpecValues)
            .HasForeignKey(a => a.SpecDefinitionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<AssetSpecValue>()
            .HasOne(a => a.SpecValue)
            .WithMany()
            .HasForeignKey(a => a.SpecValueId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<VwEmployee>().ToView("vw_Employees").HasKey(e => e.Id);
        builder.Entity<VwDepartment>().ToView("vw_Departments").HasKey(d => d.Id);
        builder.Entity<Asset>()
            .HasOne(a => a.Vendor)
            .WithMany(v => v.Assets)
            .HasForeignKey(a => a.VendorId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<VwPurchaseRequest>()
            .ToView("vw_PurchaseRequests")
            .HasNoKey();

        builder.Entity<AssemblyComponent>()
            .HasOne(ac => ac.PcAsset)
            .WithMany(a => a.Components)
            .HasForeignKey(ac => ac.PcAssetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<AssemblyComponent>()
            .HasOne(ac => ac.ComponentAsset)
            .WithMany(a => a.AsComponentOf)
            .HasForeignKey(ac => ac.ComponentAssetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
