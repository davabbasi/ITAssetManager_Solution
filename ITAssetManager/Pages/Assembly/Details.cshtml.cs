using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assembly;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public DetailsModel(ApplicationDbContext context) => _context = context;

    public Asset PcAsset { get; set; } = null!;
    public List<AssemblyComponent> ActiveComponents { get; set; } = new();
    public List<AssemblyComponent> RemovedComponents { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var pc = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id && a.IsAssembled);
        if (pc == null) return NotFound();
        PcAsset = pc;

        var components = await _context.AssemblyComponents
            .Include(c => c.ComponentAsset)
                .ThenInclude(a => a!.Category)
            .Where(c => c.PcAssetId == id)
            .OrderByDescending(c => c.InstalledAt)
            .ToListAsync();

        ActiveComponents = components.Where(c => c.RemovedAt == null).ToList();
        RemovedComponents = components.Where(c => c.RemovedAt != null).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostRemoveComponentAsync(int componentId)
    {
        var comp = await _context.AssemblyComponents.FindAsync(componentId);
        if (comp != null)
        {
            comp.RemovedAt = DateTime.Now;
            comp.RemovedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();
            TempData["Success"] = "قطعه از سیستم خارج شد و آماده استفاده مجدد است.";
        }
        return RedirectToPage(new { id = comp!.PcAssetId });
    }
}