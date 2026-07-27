namespace ITAssetManager.Models.DTOs
{
    public class AssetEditModel
    {
        public int Id { get; set; }

        // اطلاعات اصلی
        public string Name { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? Barcode { get; set; }
        public string? PropertyTag { get; set; } // برچسب اموال واحد مالی

        // دسته‌بندی
        public int CategoryId { get; set; }

        // وضعیت
        public AssetStatus Status { get; set; } = AssetStatus.Active;
        public string? StatusNote { get; set; }

        // تاریخ‌ها
        public string? PurchaseDate { get; set; }
        public string? WarrantyExpiry { get; set; }
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
      

      
        //فقط برای اسمبل
        public int? AssemblyNumber { get; set; }   // شماره شناسایی PC (فقط برای PCهای اسمبل‌شده)
        public bool IsAssembled { get; set; } = false; // آیا این Asset یک PC اسمبلیه؟

      
    }
}
