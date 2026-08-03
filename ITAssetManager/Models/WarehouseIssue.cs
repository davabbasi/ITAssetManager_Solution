using Microsoft.AspNetCore.Http.HttpResults;

namespace ITAssetManager.Models
{
    public class WarehouseIssue
    {
        public int Id { get; set; }
        public int IssueNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? Description { get; set; }
        public bool Status { get; set; }

        public ICollection<WarehouseIssueItem> Items { get; set; } = new List<WarehouseIssueItem>();
    }
}
