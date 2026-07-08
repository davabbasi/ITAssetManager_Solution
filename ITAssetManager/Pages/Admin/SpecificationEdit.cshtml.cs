using ITAssetManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ITAssetManager.Pages.Admin;

[Authorize(Policy = "RequireAdminRole")]
public class SpecificationEdit : PageModel
{
    private readonly ApplicationDbContext _context;
    public SpecificationEdit(ApplicationDbContext context) => _context = context;

    public int SpecId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var spec = await _context.Specifications.FindAsync(id);
        if (spec == null) return NotFound();
        SpecId = spec.Id;   
        Name = spec.Name;
        SortOrder = spec.SortOrder;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id,string name, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            return RedirectToPage(new { id });

        var spec = await _context.Specifications.FindAsync(id);
        if (spec != null)
        {
            spec.Name = name.Trim();
            spec.SortOrder = sortOrder;
            await _context.SaveChangesAsync();
        }
        TempData["Success"] = "مشخصه با موفقیت ویرایش شد.";
        return RedirectToPage("/Admin/SpecificationIndex");
    }
}
