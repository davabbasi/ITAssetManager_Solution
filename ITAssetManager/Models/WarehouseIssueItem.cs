using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManager.Models
{
    public class WarehouseIssueItem
    {
        public int Id { get; set; }
        public int RowNumber { get; set; }
        public int Quantity { get; set; }

        public  WarehouseIssue? Issue { get; set; }

        [ForeignKey(nameof(Issue))]
        public int IssueId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }
        public int ProductId { get; set; }

    }
}
