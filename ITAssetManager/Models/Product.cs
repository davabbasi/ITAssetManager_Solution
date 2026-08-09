using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManager.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "نام کالا")]
        [Required(ErrorMessage = "لطفا {0} را وارد نمایید")]
        [MaxLength(200, ErrorMessage = "{0} نمیتواند بیشتر از {1} باشد")]
        public string? ProductName { get; set; }

        [Display(Name = "شرح کالا")]
        public string? ProductDescription { get; set; }

        [Display(Name = "مدل")]
        [MaxLength(200, ErrorMessage = "{0} نمیتواند بیشتر از {1} باشد")]
        public string? Model { get; set; }

        [Display(Name = "دسته بندی")]
        public Category? Category { get; set; }

        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }

        public ICollection<WarehouseReceiptItem> Receipts { get; set; } = new List<WarehouseReceiptItem>();
        public ICollection<WarehouseIssueItem> Issues { get; set; } = new List<WarehouseIssueItem>();


    }
}
