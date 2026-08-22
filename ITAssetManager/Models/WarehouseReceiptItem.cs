using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManager.Models
{
    public class WarehouseReceiptItem
    {
        public int Id { get; set; }
        public int RowNumber { get; set; }
        public int Quantity { get; set; }

        public  WarehouseReceipt? Receipt { get; set; }
        [ForeignKey(nameof(Receipt))]
        public int? ReceiptId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }
        public int ProductId { get; set; }

        public string? Description { get; set; }

    }
}
