using System.ComponentModel.DataAnnotations;

namespace ITAssetManager.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssetCategoryType? Type { get; set; } = AssetCategoryType.Tagged;
    public Boolean HasInternalComponent { get; set; } = false;
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<CategorySpecification> CategorySpecifications { get; set; } = new List<CategorySpecification>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public enum AssetCategoryType
{
    [Display(Name = " مستقل")]
    Tagged = 1,

    [Display(Name = " قطعه")]
    Installed = 2,

    [Display(Name = " مصرفی")]
    Consumable = 3,    

    [Display(Name = "مستقل/اسمبلی")]
    Tagged_Installed = 4  

}