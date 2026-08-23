using System.ComponentModel.DataAnnotations;

namespace ITAssetManager.Models
{
    public enum TransferSource
    {
        [Display(Name = "انتقال دستی")]
        Manual = 1,

        [Display(Name = "ایجاد تجهیز")]
        AssetCreation = 2,

        [Display(Name = "اسقاط")]
        Scrap = 3,

        [Display(Name = "ضایعات")]
        Waste = 4,

    }
    public enum DocumentStatus
    {
        [Display(Name = "پیش‌ نویس")]
        Draft = 1,

        [Display(Name = "نهایی شده")]
        Posted = 2,

        [Display(Name = "لغو شده")]
        Cancelled = 3
    }

    public class WarehouseTransfer
    {
        public int Id { get; set; }

        [Display(Name = "شماره انتقال")]
        public int TransferNumber { get; set; }
        public int SourceWarehouseId { get; set; }

        [Display(Name = "انبار مبدا")]
        public Warehouse? SourceWarehouse { get; set; } = null!;

        public int DestinationWarehouseId { get; set; }

        [Display(Name = "انبار مقصد")]
        public Warehouse? DestinationWarehouse { get; set; } = null!;

        [Display(Name = "تاریخ انتقال")]
        public DateTime TransferDate { get; set; }

        [Display(Name = "وضعیت")]
        public DocumentStatus Status { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "ایجاد کننده")]
        public string? CreatedBy { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "نوع انتقال ")]
        public TransferSource TransferSource { get; set; } = TransferSource.Manual;

        public ICollection<WarehouseTransferItem> Items { get; set; }
            = new List<WarehouseTransferItem>();
    }
}
