using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Reports;

[Authorize]
public class ComponentHistoryModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public ComponentHistoryModel(ApplicationDbContext context) => _context = context;

    public Asset Asset { get; set; } = null!;
    public List<AssemblyComponent> History { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var asset = await _context.Assets
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (asset == null) return NotFound();
        Asset = asset;

        History = await _context.AssemblyComponents
            .Include(c => c.PcAsset)
            .Where(c => c.ComponentAssetId == id)
            .OrderByDescending(c => c.InstalledAt)
            .ToListAsync();

        return Page();
    }
}