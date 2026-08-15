using System.ComponentModel.DataAnnotations;

namespace ITAssetManager.Models
{
    public enum InventoryTransactionType
    {
        [Display(Name = "رسید")]
        Receipt = 1,

        [Display(Name = "حواله")]
        Issue = 2,

        [Display(Name = "انتقال به انبار")]
        TransferIn = 3,

        [Display(Name = "انتقال از انبار")]
        TransferOut = 4,

        [Display(Name = " (افزایش)اصلاح موجودی")]
        AdjustmentIn = 5,

        [Display(Name = " (کاهش)اصلاح موجودی")]
        AdjustmentOut = 6,

        [Display(Name = " موجودی اولیه")]
        OpeningBalance = 7
    }
    public class InventoryTransaction
    {
        public long Id { get; set; }

        public int WarehouseId { get; set; }

        public int ProductId { get; set; }
        [Display(Name = "تعداد")]
        public decimal Quantity { get; set; }
        [Display(Name = "نوع تراکنش")]
        public InventoryTransactionType Type { get; set; }
        [Display(Name = "تاریخ تراکنش")]
        public DateTime TransactionDate { get; set; }

        public int? ReceiptItemId { get; set; }

        public int? IssueItemId { get; set; }
        public int? TransferItemId { get; set; }
        [Display(Name = "شرح تراکنش")]
        public string? Description { get; set; }
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "ایجاد کننده")]
        public string CreatedBy { get; set; }
        [Display(Name = "انبار")]
        public Warehouse Warehouse { get; set; } = null!;
        [Display(Name = "نام کالا")]
        public Product Product { get; set; } = null!;
    }
}
