namespace ITAssetManager.Models
{
    public class WarehouseTransferItem
    {
        public int Id { get; set; }

        public int WarehouseTransferId { get; set; }
        public int RowNumber { get; set; }

        public WarehouseTransfer? WarehouseTransfer { get; set; } = null!;

        public int ProductId { get; set; }

        public Product? Product { get; set; } = null!;

        public decimal Quantity { get; set; }

        public string? Description { get; set; }
    }
}
