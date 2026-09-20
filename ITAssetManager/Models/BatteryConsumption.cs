using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManager.Models
{
    public class BatteryConsumption
    {
        public int Id { get; set; }

        // UPS که باتری برای آن مصرف شده
        [Required]
        [ForeignKey(nameof(UPS))]
        public int UPS_Id { get; set; }
        public Asset? UPS { get; set; }

        // کالای UPS
        [Required]
        public int ProductId { get; set; }

        public Product? Product { get; set; }

        // تعداد مصرف شده
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "تعداد باید حداقل 1 باشد.")]
        public int Quantity { get; set; }

        // تاریخ مصرف
        [Required]
        public DateTime ConsumptionDate { get; set; } = DateTime.Now;

        // کاربر ثبت کننده
        public string? CreatedBy { get; set; }

        // تاریخ ثبت رکورد
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // توضیحات
        public string? Description { get; set; }

        // ارتباط با حواله انبار
        public int? WarehouseIssueId { get; set; }

        public WarehouseIssue? WarehouseIssue { get; set; }
    }
}
