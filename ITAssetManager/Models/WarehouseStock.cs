using System.ComponentModel.DataAnnotations;

namespace ITAssetManager.Models
{
    public class WarehouseStock
    {
        public int Id { get; set; }

        public int WarehouseId { get; set; }

        public int ProductId { get; set; }
        [Display(Name = "تعداد")]
        public decimal Quantity { get; set; }
        [Display(Name = "تاریخ به روز رسانی")]
        public DateTime UpdatedAt { get; set; }
        [Display(Name = "نام انبار")]
        public Warehouse Warehouse { get; set; } = null!;
        [Display(Name = "نام کالا")]
        public Product Product { get; set; } = null!;
    }
}
