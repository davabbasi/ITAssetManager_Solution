using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManager.Models
{
    public class Warehouse
    {
        public int Id { get; set; }

        [Display(Name = "نام انبار")]
        [Required(ErrorMessage = "لطفا {0} را وارد نمایید")]
        [MaxLength(200, ErrorMessage = "{0} نمیتواند بیشتر از {1} باشد")]
        public string WarehouseName { get; set; } = string.Empty;

        [Display(Name = "توضیحات")]
        [MaxLength(400, ErrorMessage = "{0} نمیتواند بیشتر از {1} باشد")]
        public string? Description { get; set; }

        [ForeignKey(nameof(Keeper))]
        [Display(Name = "انباردار")]
        [Required(ErrorMessage = "لطفاً انباردار را انتخاب کنید.")]
        [Range(1, int.MaxValue, ErrorMessage = "لطفاً انباردار را انتخاب کنید.")]
        public int KeeperId { get; set; }
        public WarehouseKeeper? Keeper { get; set; }

        public ICollection<WarehouseReceipt> Receipts { get; set; }= new List<WarehouseReceipt>();
        public ICollection<WarehouseIssue> OutgoingIssues { get; set; }= new List<WarehouseIssue>();
        public ICollection<WarehouseIssue> IncomingIssues { get; set; }= new List<WarehouseIssue>();

    }
}
