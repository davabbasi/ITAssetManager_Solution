using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Vendors;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public DetailsModel(ApplicationDbContext context) => _context = context;

    public Vendor Vendor { get; set; } = null!;
    public List<Asset> Assets { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null) return NotFound();
        Vendor = vendor;

        Assets = await _context.Assets
            .Include(a => a.Category)
            .Where(a => a.VendorId == id)
            .OrderBy(a => a.Name)
            .ToListAsync();

        return Page();
    }
}