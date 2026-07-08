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
    public string SpecName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int specId)
    {
        var spec = await _context.Specifications.FindAsync(specId);
        if (spec == null) return NotFound();

        SpecId = specId;
        SpecName = spec.Name;
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
        }

        // اگر "ذخیره و جدید" زده شد، دوباره همین صفحه
        if (action == "saveAndNew")
            return RedirectToPage(new { specId });

        return RedirectToPage("/Admin/SpecificationIndex", new { selectedSpecId = specId });
    }
}
