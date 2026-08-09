using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ITAssetManager.Models
{
    public class WarehouseReceipt
    {
        public int Id { get; set; }

        [Display(Name = "شماره رسید")]
        public int ReceiptNumber { get; set; }
        [Display(Name = "تاریخ رسید")]
        public DateTime ReceiptDate { get; set; }
        [Display(Name = "شماره درخواست خرید")]
        public string? ReferenceNumber { get; set; }
        [Display(Name = "ثبت کننده")]
        public string CreatedBy { get; set; } = string.Empty;
        [Display(Name = "تاریخ ثبت")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }
        [Display(Name = "وضعیت")]
        public bool Status { get; set; } = true;

        public ICollection<WarehouseReceiptItem> Items { get; set; } = new List<WarehouseReceiptItem>();
        public Warehouse? Warehouse { get; set; }

        [ForeignKey(nameof(Warehouse))]
        [Display(Name = "انبار")]

        public int WarehouseId { get; set; }

    }
}
