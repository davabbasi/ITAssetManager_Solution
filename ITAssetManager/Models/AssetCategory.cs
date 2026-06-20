namespace ITAssetManager.Models;

public class AssetCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; } = "bi-cpu";
    public string? Description { get; set; }
    public AssetCategoryType? Type { get; set; } = AssetCategoryType.Tagged;
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}

public enum AssetCategoryType
{
    Tagged = 1,       // کالای برچسبی
    Installed = 2,    // قطعات نصبی
    Consumable = 3    // مواد مصرفی
}