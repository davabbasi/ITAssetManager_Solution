namespace ITAssetManager.Models
{
    public enum InventoryTransactionType
    {
        Receipt = 1,
        Issue = 2,
        TransferIn = 3,
        TransferOut = 4,
        AdjustmentIn = 5,
        AdjustmentOut = 6,
        OpeningBalance = 7
    }
    public class InventoryTransaction
    {
        public long Id { get; set; }

        public int WarehouseId { get; set; }

        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        public InventoryTransactionType Type { get; set; }

        public DateTime TransactionDate { get; set; }

        public int? ReceiptItemId { get; set; }

        public int? IssueItemId { get; set; }
        public int? TransferItemId { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public Warehouse Warehouse { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
