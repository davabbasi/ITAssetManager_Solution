using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManager.Models
{
    public class Specification
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; } = 0;

        public ICollection<CategorySpecification> CategorySpecifications { get; set; } = new List<CategorySpecification>();
        public ICollection<SpecificationValue> SpecValues { get; set; } = new List<SpecificationValue>();
        public ICollection<AssetSpecificationValue> AssetSpecValues { get; set; } = new List<AssetSpecificationValue>();
    }
}
