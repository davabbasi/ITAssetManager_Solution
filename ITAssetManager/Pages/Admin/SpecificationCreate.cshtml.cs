using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Admin;

[Authorize(Policy = "RequireAdminRole")]
public class SpecDefinitionCreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public SpecDefinitionCreateModel(ApplicationDbContext context) => _context = context;

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int categoryId)
    {
        CategoryId = categoryId;
        var cat = await _context.Categories.FindAsync(categoryId);
        if (cat == null) return NotFound();
        CategoryName = cat.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int categoryId, string name, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            return RedirectToPage(new { categoryId });

        _context.Specifications.Add(new Specification
        {
            Name = name.Trim(),
            SortOrder = sortOrder
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = $"مشخصه «{name}» با موفقیت ایجاد شد.";
        return RedirectToPage("/Admin/Specs", new { categoryId });
    }
}
