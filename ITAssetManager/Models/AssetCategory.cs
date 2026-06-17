namespace ITAssetManager.Models;

public class AssetCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; } = "bi-cpu";
    public string? Description { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
