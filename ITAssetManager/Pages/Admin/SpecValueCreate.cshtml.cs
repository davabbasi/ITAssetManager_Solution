using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ITAssetManager.Pages.Admin;

[Authorize(Policy = "RequireAdminRole")]
public class SpecValueCreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public SpecValueCreateModel(ApplicationDbContext context) => _context = context;

    public int SpecId { get; set; }
    public int CategoryId { get; set; }
    public string SpecName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int specId, int categoryId)
    {
        var spec = await _context.SpecDefinitions.FindAsync(specId);
        if (spec == null) return NotFound();

        SpecId = specId;
        CategoryId = categoryId;
        SpecName = spec.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int specId, int categoryId,
        string value, int sortOrder = 0, string action = "save")
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            _context.SpecValues.Add(new SpecValue
            {
                Value = value.Trim(),
                SpecDefinitionId = specId,
                SortOrder = sortOrder
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"مقدار «{value}» با موفقیت ایجاد شد.";
        }

        // اگر "ذخیره و جدید" زده شد، دوباره همین صفحه
        if (action == "saveAndNew")
            return RedirectToPage(new { specId, categoryId });

        return RedirectToPage("/Admin/Specs", new { categoryId, selectedSpecId = specId });
    }
}
