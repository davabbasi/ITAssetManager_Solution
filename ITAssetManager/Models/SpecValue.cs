namespace ITAssetManager.Models
{
    public class SpecValue
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public int SpecDefinitionId { get; set; }
        public int SortOrder { get; set; } = 0;

        public SpecDefinition? SpecDefinition { get; set; }
    }
}
