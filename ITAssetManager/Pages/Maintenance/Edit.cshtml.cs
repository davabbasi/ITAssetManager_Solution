using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Maintenance;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public EditModel(ApplicationDbContext context) => _context = context;

    [BindProperty] public MaintenanceLog Log { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var log = await _context.MaintenanceLogs
            .Include(m => m.Asset)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (log == null) return NotFound();
        Log = log;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Log.Asset");
        if (!ModelState.IsValid) return Page();

        _context.Attach(Log).State = EntityState.Modified;

        // اگر مشکل حل شد، وضعیت تجهیز را به فعال برگردان
        if (Log.IsResolved)
        {
            var asset = await _context.Assets.FindAsync(Log.AssetId);
            if (asset?.Status == AssetStatus.UnderRepair)
                asset.Status = AssetStatus.Active;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "تعمیر با موفقیت ویرایش شد.";
        return RedirectToPage("/Assets/Details", new { id = Log.AssetId });
    }
}
