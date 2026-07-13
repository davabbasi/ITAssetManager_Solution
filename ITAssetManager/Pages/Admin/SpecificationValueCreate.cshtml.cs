using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Admin;

[Authorize(Policy = "RequireAdminRole")]
public class SpecValueCreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public SpecValueCreateModel(ApplicationDbContext context) => _context = context;

    [BindProperty]
    public int SpecId { get; set; }
    public string SpecName { get; set; } = string.Empty;
    public List<SpecificationValue> SpecValues { get; set; }
    public async Task<IActionResult> OnGetAsync(int specId)
    {
        var spec = await _context.Specifications.FindAsync(specId);
        if (spec == null) return NotFound();

        SpecId = specId;
        SpecName = spec.Name;
        SpecValues = await _context.SpecValues.Include(v=>v.Specification).Where(v => v.SpecificationId == specId).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int specId,
        string value, int sortOrder = 0, string action = "save")
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            _context.SpecValues.Add(new SpecificationValue
            {
                Value = value.Trim(),
                SpecificationId = specId,
                SortOrder = sortOrder
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"مقدار «{value}» با موفقیت ایجاد شد.";
            return RedirectToPage(new { specId });

        }

        return RedirectToPage("/Admin/SpecificationIndex", new { selectedSpecId = specId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, int specId)
    {
        var specValue = await _context.SpecValues
            .Include(s => s.AssetSpecificationValues)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (specValue == null)
            return NotFound();

        if (specValue.AssetSpecificationValues.Any())
        {
            TempData["Error"] = "این مقدار در برخی از تجهیزات مورد استفاده قرار گرفته است";
            return RedirectToPage();
        }

        _context.SpecValues.Remove(specValue);
        await _context.SaveChangesAsync();

        TempData["Success"] = "مقدار  با موفقیت حذف شد.";

        return RedirectToPage(new { specId });
    }
}
