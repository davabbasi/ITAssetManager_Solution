namespace ITAssetManager.Models
{
    public class AssetSpecValue
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int SpecDefinitionId { get; set; }
        public int SpecValueId { get; set; }

        public Asset? Asset { get; set; }
        public SpecDefinition? SpecDefinition { get; set; }
        public SpecValue? SpecValue { get; set; }
    }
}
