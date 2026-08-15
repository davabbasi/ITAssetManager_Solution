using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ITAssetManager.Models
{
    public class WarehouseIssue
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "شماره حواله")]
        [Required(ErrorMessage = "لطفا {0} را وارد نمایید")]
        public int IssueNumber { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد نمایید")]
        [Display(Name = "تاریخ حواله")]
        public DateTime IssueDate { get; set; }

        [Display(Name = "ثبت کننده")]
        public string? CreatedBy { get; set; }

        public int? EmployeeId { get; set; }

        [Display(Name = "تحویل گیرنده ")]
        public string? EmployeeName { get; set; }


        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "وضعیت")]
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

        public ICollection<WarehouseIssueItem> Items { get; set; } = new List<WarehouseIssueItem>();
        public Warehouse? Warehouse { get; set; }

        [ForeignKey(nameof(Warehouse))]
        [Display(Name = "انبار مبدا")]
        public int WarehouseId { get; set; }

       

    }
}
