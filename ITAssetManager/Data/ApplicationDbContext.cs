using System.Reflection.Emit;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetAssignment> AssetAssignments { get; set; }
    public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
    public DbSet<Specification> Specifications { get; set; }
    public DbSet<SpecificationValue> SpecValues { get; set; }
    public DbSet<AssetSpecificationValue> AssetSpecValues { get; set; }
    public DbSet<VwEmployee> VwEmployees { get; set; }
    public DbSet<VwDepartment> VwDepartments { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<VwPurchaseRequest> VwPurchaseRequests { get; set; }
    public DbSet<AssemblyComponent> AssemblyComponents { get; set; }
    public DbSet<CategorySpecification> CategorySpecifications { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<WarehouseReceipt> WarehouseReceipts { get; set; }
    public DbSet<WarehouseReceiptItem> WarehouseReceiptItems { get; set; }
    public DbSet<WarehouseIssue> WarehouseIssues { get; set; }
    public DbSet<WarehouseIssueItem> WarehouseIssueItems { get; set; }
    public DbSet<WarehouseKeeper> WarehouseKeepers { get; set; }
    public DbSet<WarehouseStock> WarehouseStocks { get; set; }
    public DbSet<WarehouseTransfer> WarehouseTransfers { get; set; }
    public DbSet<WarehouseTransferItem> WarehouseTransferItems { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }



    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);



        // Seed: دسته‌بندی‌های پیش‌فرض
        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "لپ‌تاپ"},
            new Category { Id = 2, Name = "کامپیوتر رومیزی"},
            new Category { Id = 3, Name = "مانیتور" },
            new Category { Id = 4, Name = "پرینتر"},
            new Category { Id = 5, Name = "ماوس" },
            new Category { Id = 6, Name = "کیبورد"},
            new Category { Id = 7, Name = "سوئیچ شبکه"},
            new Category { Id = 8, Name = "روتر" },
            new Category { Id = 9, Name = "سرور"},
            new Category { Id = 10, Name = "UPS" },
            new Category { Id = 11, Name = "هدست"},
            new Category { Id = 12, Name = "سایر"}
        );

        builder.Entity<Asset>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Asset>()
            .HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Asset>()
            .HasOne(x => x.WarehouseIssue)
            .WithMany()
            .HasForeignKey(x => x.WarehouseIssueId)
            .OnDelete(DeleteBehavior.Restrict);

        // InventoryTransaction
        builder.Entity<InventoryTransaction>()
            .HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InventoryTransaction>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);


        // WarehouseStock
        builder.Entity<WarehouseStock>()
            .HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WarehouseStock>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);


        // WarehouseTransfer
        builder.Entity<WarehouseTransfer>()
            .HasOne(x => x.SourceWarehouse)
            .WithMany()
            .HasForeignKey(x => x.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WarehouseTransfer>()
            .HasOne(x => x.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(x => x.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WarehouseIssue>()
            .HasOne(x => x.Warehouse)
            .WithMany(x => x.OutgoingIssues)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

       

        builder.Entity<Warehouse>()
           .HasOne(r => r.Keeper)
           .WithMany(r => r.Warehouses)
           .HasForeignKey(r => r.KeeperId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WarehouseIssueItem>()
            .HasOne(r => r.Product)
            .WithMany(r => r.Issues)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WarehouseReceiptItem>()
            .HasOne(r=>r.Product)
            .WithMany(r=>r.Receipts)
            .HasForeignKey(r=>r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WarehouseReceipt>()
            .HasOne(r=>r.Warehouse)
            .WithMany(r=>r.Receipts)
            .HasForeignKey(r=>r.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WarehouseReceiptItem>()
            .HasOne(r=>r.Receipt)
            .WithMany(r=>r.Items)
            .HasForeignKey(r=>r.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WarehouseIssueItem>()
             .HasOne(r => r.Issue)
             .WithMany(r => r.Items)
             .HasForeignKey(r => r.IssueId)
             .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Asset>()
            .HasOne(a=>a.Category)
            .WithMany(c=>c.Assets)
            .HasForeignKey(a=>a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SpecificationValue>()
        .HasOne(v => v.Specification)
        .WithMany(s => s.SpecValues)
        .HasForeignKey(v => v.SpecificationId)
        .OnDelete(DeleteBehavior.Restrict);



        builder.Entity<CategorySpecification>()
            .HasOne(cs => cs.Category)
            .WithMany(c => c.CategorySpecifications)
            .HasForeignKey(cs => cs.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CategorySpecification>()
            .HasOne(cs => cs.Specification)
            .WithMany(s => s.CategorySpecifications)
            .HasForeignKey(cs => cs.SpecificationId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Entity<AssetSpecificationValue>()
            .HasOne(asv => asv.Asset)
            .WithMany(a => a.SpecValues)
            .HasForeignKey(asv =>asv.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AssetSpecificationValue>()
            .HasOne(asv => asv.Specification)
            .WithMany(s => s.AssetSpecValues)
            .HasForeignKey(asv => asv.SpecDefinitionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<AssetSpecificationValue>()
            .HasOne(asv => asv.SpecValue)
            .WithMany(sv=>sv.AssetSpecificationValues)
            .HasForeignKey(asv => asv.SpecValueId)
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

        builder.Entity<Product>()
           .HasOne(p=>p.Category)
           .WithMany(a => a.Products)
           .HasForeignKey(p=>p.CategoryId)
           .OnDelete(DeleteBehavior.Restrict);
    }
}
