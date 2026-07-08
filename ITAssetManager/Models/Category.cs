namespace ITAssetManager.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssetCategoryType? Type { get; set; } = AssetCategoryType.Tagged;
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<CategorySpecification> CategorySpecifications { get; set; } = new List<CategorySpecification>();
}

public enum AssetCategoryType
{
    Tagged = 1,       // کالای برچسبی
    Installed = 2,    // قطعات نصبی
    Consumable = 3    // مواد مصرفی
}