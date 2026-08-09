namespace ITAssetManager.Models
{
    public enum DocumentStatus
    {
        Draft = 1,
        Posted = 2,
        Cancelled = 3
    }
    public class WarehouseTransfer
    {
        public int Id { get; set; }

        /// <summary>
        /// شماره انتقال
        /// </summary>
        public int TransferNumber { get; set; }

        /// <summary>
        /// انبار مبدا
        /// </summary>
        public int SourceWarehouseId { get; set; }

        public Warehouse? SourceWarehouse { get; set; } = null!;

        /// <summary>
        /// انبار مقصد
        /// </summary>
        public int DestinationWarehouseId { get; set; }

        public Warehouse? DestinationWarehouse { get; set; } = null!;

        /// <summary>
        /// تاریخ انتقال
        /// </summary>
        public DateTime TransferDate { get; set; }

        /// <summary>
        /// وضعیت سند
        /// </summary>
        public DocumentStatus Status { get; set; }

        public string? Description { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<WarehouseTransferItem> Items { get; set; }
            = new List<WarehouseTransferItem>();
    }
}
