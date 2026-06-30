using System.ComponentModel.DataAnnotations;

namespace ITAssetManager.Models.ViewModels
{
    public class CategoryEditViewModel
    {

        public int Id { get; set; }

        [Display(Name = "نوع دسته بندی")]
        [Required(ErrorMessage = "لطفا {0} را وارد نمایید")]
        public int Type { get; set; } = 1;

        [Display(Name = "نام دسته بندی")]
        [Required(ErrorMessage = "لطفا {0} را وارد نمایید")]
        [MaxLength(200, ErrorMessage = "{0} نمیتواند بیشتر از {1} باشد")]
        public  string Name { get; set; }= string.Empty;

        public string? Icon { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }
    }
}
