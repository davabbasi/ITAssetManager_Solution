using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Vendors;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public EditModel(ApplicationDbContext context) => _context = context;

    [BindProperty] public Vendor Vendor { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null) return NotFound();
        Vendor = vendor;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Vendor.Assets");
        if (!ModelState.IsValid) return Page();

        _context.Attach(Vendor).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        TempData["Success"] = "تغییرات با موفقیت ذخیره شد.";
        return RedirectToPage("/Vendors/Details", new { id = Vendor.Id });
    }
}