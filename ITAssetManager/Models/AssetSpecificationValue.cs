namespace ITAssetManager.Models
{
    public class AssetSpecificationValue
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int SpecDefinitionId { get; set; }
        public int SpecValueId { get; set; }

        public Asset? Asset { get; set; }
        public Specification? Specification { get; set; }
        public SpecificationValue? SpecValue { get; set; }
    }
}
