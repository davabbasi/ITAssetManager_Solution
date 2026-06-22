using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Admin;

[Authorize(Policy = "RequireAdminRole")]
public class SpecsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public SpecsModel(ApplicationDbContext context) => _context = context;

    public List<AssetCategory> Categories { get; set; } = new();
    public List<SpecDefinition> SpecDefinitions { get; set; } = new();
    public List<SpecValue> SpecValues { get; set; } = new();
    public int? SelectedCategoryId { get; set; }
    public int? SelectedSpecId { get; set; }
    public string? SelectedSpecName { get; set; }

    public async Task OnGetAsync()
    {
        SelectedCategoryId = int.TryParse(Request.Query["categoryId"], out var c) ? c : null;
        SelectedSpecId = int.TryParse(Request.Query["selectedSpecId"], out var s) ? s : null;

        Categories = await _context.AssetCategories.OrderBy(c => c.Name).ToListAsync();

        if (SelectedCategoryId.HasValue)
        {
            SpecDefinitions = await _context.SpecDefinitions
                .Include(s => s.SpecValues)
                .Where(s => s.CategoryId == SelectedCategoryId)
                .OrderBy(s => s.SortOrder).ThenBy(s => s.Name)
                .ToListAsync();
        }

        if (SelectedSpecId.HasValue)
        {
            var spec = await _context.SpecDefinitions.FindAsync(SelectedSpecId);
            SelectedSpecName = spec?.Name;

            SpecValues = await _context.SpecValues
                .Where(v => v.SpecDefinitionId == SelectedSpecId)
                .OrderBy(v => v.SortOrder).ThenBy(v => v.Value)
                .ToListAsync();
        }
    }

    public async Task<IActionResult> OnPostDeleteSpecAsync(int specId, int categoryId)
    {
        var spec = await _context.SpecDefinitions.FindAsync(specId);
        if (spec != null) _context.SpecDefinitions.Remove(spec);
        await _context.SaveChangesAsync();
        TempData["Success"] = "مشخصه با موفقیت حذف شد.";
        return RedirectToPage(new { categoryId });
    }

    public async Task<IActionResult> OnPostDeleteValueAsync(int valueId, int categoryId, int specId)
    {
        var val = await _context.SpecValues.FindAsync(valueId);
        if (val != null) _context.SpecValues.Remove(val);
        await _context.SaveChangesAsync();
        TempData["Success"] = "مقدار با موفقیت حذف شد.";
        return RedirectToPage(new { categoryId, selectedSpecId = specId });
    }
}
