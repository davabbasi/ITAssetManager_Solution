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

    public async Task OnGetAsync()
    {
        
        Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

        if (SelectedCategoryId.HasValue)
        {
            CategorySpecifications = await _context.CategorySpecifications
                .Where(cs=>cs.CategoryId== SelectedCategoryId)
                .Include(cs => cs.Category).Include(cs=>cs.Specification)
                .OrderBy(cs => cs.Specification.SortOrder).ThenBy(cs => cs.Specification.Name)
                .ToListAsync();
        }

        if (SelectedSpecId.HasValue)
        {
            SelectedSpecName = await _context.Specifications
                .Where(s => s.Id == SelectedSpecId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            SpecValues = await _context.SpecValues
                .Where(v => v.SpecificationId == SelectedSpecId)
                .OrderBy(v => v.SortOrder).ThenBy(v => v.Value)
                .ToListAsync();
        }
    }

    public async Task<IActionResult> OnPostDeleteSpecAsync(int specId, int categoryId)
    {
        var spec = await _context.Specifications.FindAsync(specId);
        if (spec == null)
            return NotFound();
        if (spec != null) _context.Specifications.Remove(spec);
        await _context.SaveChangesAsync();
        TempData["Success"] = "مشخصه با موفقیت حذف شد.";
        return RedirectToPage(new { categoryId });
    }

    public async Task<IActionResult> OnPostDeleteValueAsync(int valueId, int categoryId, int specId)
    {
        var val = await _context.SpecValues.FindAsync(valueId);
        if (val == null)
            return NotFound();
        if (val != null) _context.SpecValues.Remove(val);
        await _context.SaveChangesAsync();
        TempData["Success"] = "مقدار با موفقیت حذف شد.";
        return RedirectToPage(new { categoryId, selectedSpecId = specId });
    }
}
