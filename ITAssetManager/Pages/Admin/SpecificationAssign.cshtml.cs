using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Admin;

[Authorize(Policy = "RequireAdminRole")]
public class SpecificationAssignToCategory : PageModel
{
    private readonly ApplicationDbContext _context;
    public SpecificationAssignToCategory(ApplicationDbContext context) => _context = context;

    public List<Category> Categories { get; set; } = new();
    public List<CategorySpecification> CategorySpecifications { get; set; } = new();
    public List<SpecificationValue> SpecValues { get; set; } = new();
    [BindProperty(SupportsGet = true)]
    public int? SelectedCategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SelectedSpecId { get; set; }
    public string? SelectedSpecName { get; set; }
    [BindProperty]
    public List<int> SelectedSpecifications { get; set; } = new();
    public List<SpecificationItem> Specifications { get; set; } = new();
    public class SpecificationItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool Selected { get; set; }
    }
    public async Task OnGetAsync()
    {
        
        Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        if (!SelectedCategoryId.HasValue)
            return;

        var assignedIds = await _context.CategorySpecifications
          .Where(x => x.CategoryId == SelectedCategoryId)
          .Select(x => x.SpecificationId)
          .ToListAsync();

        Specifications = await _context.Specifications
          .OrderBy(x => x.SortOrder)
          .ThenBy(x => x.Name)
          .Select(x => new SpecificationItem
          {
              Id = x.Id,
              Name = x.Name,
              Selected = assignedIds.Contains(x.Id)
          })
          .ToListAsync();


      
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!SelectedCategoryId.HasValue)
            return RedirectToPage();

        var oldRelations = await _context.CategorySpecifications
            .Where(x => x.CategoryId == SelectedCategoryId)
            .ToListAsync();

        _context.CategorySpecifications.RemoveRange(oldRelations);

        foreach (var specId in SelectedSpecifications)
        {

            _context.CategorySpecifications.Add(new CategorySpecification
            {
                CategoryId = SelectedCategoryId.Value,
                SpecificationId = specId
            });
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "مشخصات با موفقیت ذخیره شدند.";

        return RedirectToPage(new { SelectedCategoryId });
    }
   
}
