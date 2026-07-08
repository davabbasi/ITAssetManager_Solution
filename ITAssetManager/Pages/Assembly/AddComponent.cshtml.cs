using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assembly;

[Authorize]
public class AddComponentModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public AddComponentModel(ApplicationDbContext context) => _context = context;

    public Asset PcAsset { get; set; } = null!;
    public List<Category> InstalledCategories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int pcId)
    {
        var pc = await _context.Assets.FirstOrDefaultAsync(a => a.Id == pcId && a.IsAssembled);
        if (pc == null) return NotFound();
        PcAsset = pc;

        InstalledCategories = await _context.Categories
            .Where(c => c.Type == AssetCategoryType.Installed)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int pcId, int componentId)
    {
        _context.AssemblyComponents.Add(new AssemblyComponent
        {
            PcAssetId = pcId,
            ComponentAssetId = componentId,
            InstalledAt = DateTime.Now,
            InstalledBy = User.Identity?.Name
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "قطعه با موفقیت اضافه شد.";
        return RedirectToPage("/Assembly/Details", new { id = pcId });
    }
}