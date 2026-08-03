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
        public string WarehouseName { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(400, ErrorMessage = "{0} نمیتواند بیشتر از {1} باشد")]
        public string? Description { get; set; }

        public int? KeeperEmployeeId { get; set; }

        [Display(Name = "نام انباردار")]
        public string? KeeperEmployeeFullName { get; set; }

        public ICollection<WarehouseReceipt> Receipts { get; set; } = new List<WarehouseReceipt>();

    }
}
