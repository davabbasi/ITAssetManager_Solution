using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ITAssetManager.Models
{
    public enum IssueSource
    {
        [Display(Name = "حواله مستقیم")]
        Manual = 1,

        [Display(Name = "ایجاد تجهیز")]
        AssetCreation = 2,

        [Display(Name = "انجام اسمبل")]
        Assembly = 3
    }
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

        [Display(Name = "منبع حواله")]
        public IssueSource Source { get; set; } = IssueSource.Manual;

        public ICollection<WarehouseIssueItem> Items { get; set; } = new List<WarehouseIssueItem>();
        public Warehouse? Warehouse { get; set; }

        [ForeignKey(nameof(Warehouse))]
        [Display(Name = "انبار ")]
        public int WarehouseId { get; set; }

       

    }
}
