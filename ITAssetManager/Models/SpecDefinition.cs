namespace ITAssetManager.Models
{
    public class SpecDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int SortOrder { get; set; } = 0;

        public AssetCategory? Category { get; set; }
        public ICollection<SpecValue> SpecValues { get; set; } = new List<SpecValue>();
        public ICollection<AssetSpecValue> AssetSpecValues { get; set; } = new List<AssetSpecValue>();
    }
}
