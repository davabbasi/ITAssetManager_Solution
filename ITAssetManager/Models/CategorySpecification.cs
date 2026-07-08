using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManager.Models
{
    public class CategorySpecification
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int SpecificationId { get; set; }


        [ForeignKey (nameof(CategoryId))]
        public Category? Category { get; set; } = new Category();

        [ForeignKey (nameof(SpecificationId))]
        public Specification? Specification { get; set; } = new Specification();

    }
}
