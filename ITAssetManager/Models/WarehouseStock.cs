namespace ITAssetManager.Models
{
    public class WarehouseStock
    {
        public int Id { get; set; }

        public int WarehouseId { get; set; }

        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Warehouse Warehouse { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
