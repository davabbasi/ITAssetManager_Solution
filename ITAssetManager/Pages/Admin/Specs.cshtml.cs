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

    [BindProperty(SupportsGet = true)] public int? SelectedCategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public int? SelectedSpecId { get; set; }
    [BindProperty(SupportsGet = true)] public int? EditSpecId { get; set; }
    [BindProperty(SupportsGet = true)] public int? EditValueId { get; set; }

    public string? EditSpecName { get; set; }
    public int EditSortOrder { get; set; }
    public string? SelectedSpecName { get; set; }
    public string? EditValueText { get; set; }
    public int EditValueSortOrder { get; set; }

    public async Task OnGetAsync()
    {
        Categories = await _context.AssetCategories.OrderBy(c => c.Name).ToListAsync();

        if (SelectedCategoryId.HasValue)
        {
            SpecDefinitions = await _context.SpecDefinitions
                .Include(s => s.SpecValues)
                .Where(s => s.CategoryId == SelectedCategoryId)
                .OrderBy(s => s.SortOrder).ThenBy(s => s.Name)
                .ToListAsync();
        }

        if (EditSpecId.HasValue)
        {
            var spec = await _context.SpecDefinitions.FindAsync(EditSpecId);
            if (spec != null) { EditSpecName = spec.Name; EditSortOrder = spec.SortOrder; }
        }

        if (SelectedSpecId.HasValue)
        {
            var spec = await _context.SpecDefinitions.FindAsync(SelectedSpecId);
            SelectedSpecName = spec?.Name;

            if (EditValueId.HasValue)
            {
                var val = await _context.SpecValues.FindAsync(EditValueId);
                if (val != null) { EditValueText = val.Value; EditValueSortOrder = val.SortOrder; }
            }
        }
    }

    public async Task<IActionResult> OnPostSaveSpecAsync(int categoryId, int? specId,
        string specName, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(specName))
            return RedirectToPage(new { categoryId });

        if (specId.HasValue)
        {
            var spec = await _context.SpecDefinitions.FindAsync(specId);
            if (spec != null) { spec.Name = specName; spec.SortOrder = sortOrder; }
        }
        else
        {
            _context.SpecDefinitions.Add(new SpecDefinition
            {
                Name = specName,
                CategoryId = categoryId,
                SortOrder = sortOrder
            });
        }
        await _context.SaveChangesAsync();
        return RedirectToPage(new { categoryId });
    }

    public async Task<IActionResult> OnPostSaveValueAsync(int categoryId, int specDefinitionId,
        int? valueId, string value, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(value))
            return RedirectToPage(new { categoryId, selectedSpecId = specDefinitionId });

        if (valueId.HasValue)
        {
            var val = await _context.SpecValues.FindAsync(valueId);
            if (val != null) { val.Value = value; val.SortOrder = sortOrder; }
        }
        else
        {
            _context.SpecValues.Add(new SpecValue
            {
                Value = value,
                SpecDefinitionId = specDefinitionId,
                SortOrder = sortOrder
            });
        }
        await _context.SaveChangesAsync();
        return RedirectToPage(new { categoryId, selectedSpecId = specDefinitionId });
    }

    public async Task<IActionResult> OnPostDeleteSpecAsync(int specId, int categoryId)
    {
        var spec = await _context.SpecDefinitions.FindAsync(specId);
        if (spec != null) _context.SpecDefinitions.Remove(spec);
        await _context.SaveChangesAsync();
        return RedirectToPage(new { categoryId });
    }

    public async Task<IActionResult> OnPostDeleteValueAsync(int valueId, int categoryId, int specDefinitionId)
    {
        var val = await _context.SpecValues.FindAsync(valueId);
        if (val != null) _context.SpecValues.Remove(val);
        await _context.SaveChangesAsync();
        return RedirectToPage(new { categoryId, selectedSpecId = specDefinitionId });
    }
}