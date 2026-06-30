namespace ITAssetManager.Models;

public enum AssetStatus
{
    Active = 1,
    Faulty = 2,
    UnderRepair = 3,
    Scrapped = 4,
    InStorage = 5
}

public class Asset
{
    public int Id { get; set; }

    // اطلاعات اصلی
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Barcode { get; set; }
    public string? PropertyTag { get; set; } // برچسب اموال واحد مالی

    // دسته‌بندی
    public int CategoryId { get; set; }
    public AssetCategory? Category { get; set; }

    // وضعیت
    public AssetStatus Status { get; set; } = AssetStatus.Active;
    public string? StatusNote { get; set; }

    // تاریخ‌ها
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public decimal? PurchasePrice { get; set; }

    // مکان فعلی
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? Location { get; set; } // اتاق/طبقه

    // اطلاعات تکمیلی
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    //فروشنده
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public ICollection<AssetSpecValue> SpecValues { get; set; } = new List<AssetSpecValue>();
    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
    public ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();

    // اضافه کن:
    public int? AssemblyNumber { get; set; }   // شماره شناسایی PC (فقط برای PCهای اسمبل‌شده)
    public bool IsAssembled { get; set; } = false; // آیا این Asset یک PC اسمبلیه؟

    public ICollection<AssemblyComponent> AsComponentOf { get; set; } = new List<AssemblyComponent>(); // وقتی این قطعه توی یک PC استفاده شده
    public ICollection<AssemblyComponent> Components { get; set; } = new List<AssemblyComponent>();     // وقتی این خودش PC هست و قطعات داره
}
