namespace ITAssetManager.Models
{
    public class SpecificationValue
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public int SpecificationId { get; set; }
        public int SortOrder { get; set; } = 0;

        public Specification? Specification { get; set; }
        public ICollection<AssetSpecificationValue>? AssetSpecificationValues { get; set; }
    }
}
