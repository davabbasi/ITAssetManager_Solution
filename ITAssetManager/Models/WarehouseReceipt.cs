using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ITAssetManager.Models
{
    public class WarehouseReceipt
    {
        public int Id { get; set; }
        public int ReceiptNumber { get; set; }
        public DateTime ReceiptDate { get; set; }
        public int WarehouseKeeperId { get; set; }
        public string? ReferenceNumber { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? Description { get; set; }
        public bool Status { get; set; } = true;

        public ICollection<WarehouseReceiptItem> Items { get; set; } = new List<WarehouseReceiptItem>();
        public Warehouse? Warehouse { get; set; }
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }

    }
}
