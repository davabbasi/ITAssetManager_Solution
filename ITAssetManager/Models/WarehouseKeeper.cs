using System.ComponentModel.DataAnnotations;

namespace ITAssetManager.Models
{
    public class WarehouseKeeper
    {
        public int Id { get; set; }

        [Display(Name ="نام انباردار")]
        [Required(ErrorMessage = "لطفا {0} را وارد نمایید")]
        [MaxLength(200, ErrorMessage = "{0} نمیتواند بیشتر از {1} باشد")]
        public string? FullName { get; set; }

        [Display(Name = "شماره پرسنلی")]
        [MaxLength(11, ErrorMessage = "{0} نمیتواند بیشتر از {1} باشد")]
        public string? PersonnelNumber { get; set; }
        public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();


       
    }
}
