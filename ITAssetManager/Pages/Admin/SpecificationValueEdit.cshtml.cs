using ITAssetManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ITAssetManager.Pages.Admin;

[Authorize(Policy = "RequireAdminRole")]
public class SpecValueEditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public SpecValueEditModel(ApplicationDbContext context) => _context = context;

    public int ValueId { get; set; }
    public int SpecId { get; set; }
    public int CategoryId { get; set; }
    public string SpecName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, int specId)
    {
        var val = await _context.SpecValues.FindAsync(id);
        if (val == null) return NotFound();

        var spec = await _context.Specifications.FindAsync(specId);
        if (spec == null) return NotFound();

        ValueId = val.Id;
        SpecId = specId;
        SpecName = spec.Name;
        Value = val.Value;
        SortOrder = val.SortOrder;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, int specId,string value, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(value))
            return RedirectToPage(new { id, specId });

        var val = await _context.SpecValues.FindAsync(id);
        if (val != null)
        {
            val.Value = value.Trim();
            val.SortOrder = sortOrder;
            await _context.SaveChangesAsync();
        }
        TempData["Success"] = "مقدار با موفقیت ویرایش شد.";
        return RedirectToPage("/Admin/SpecificationValueCreate", new { SpecId = specId });
    }
}
