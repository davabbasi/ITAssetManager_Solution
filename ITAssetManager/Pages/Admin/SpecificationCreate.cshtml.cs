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

    public  IActionResult OnGetAsync()
    {
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync( string name, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest();

        _context.Specifications.Add(new Specification
        {
            Name = name.Trim(),
            SortOrder = sortOrder
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = $"مشخصه «{name}» با موفقیت ایجاد شد.";
        return RedirectToPage("/Admin/SpecificationIndex");
    }
}
