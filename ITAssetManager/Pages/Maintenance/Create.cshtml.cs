using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Maintenance;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public CreateModel(ApplicationDbContext context) => _context = context;

    [BindProperty] public MaintenanceLog Log { get; set; } = new();
    public Asset? Asset { get; set; }
    public SelectList AssetList { get; set; } = null!;

    public async Task OnGetAsync(int? assetId)
    {
        if (assetId.HasValue)
        {
            Asset = await _context.Assets.FindAsync(assetId);
            if (Asset != null) Log.AssetId = Asset.Id;
        }
        Log.StartDate = DateTime.Today;
        await LoadSelectListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Log.Asset");
        if (!ModelState.IsValid)
        {
            if (Log.AssetId > 0)
                Asset = await _context.Assets.FindAsync(Log.AssetId);
            await LoadSelectListAsync();
            return Page();
        }

        Log.LoggedBy = User.Identity?.Name;
        _context.MaintenanceLogs.Add(Log);

        // اگر مشکل در جریان است، وضعیت تجهیز را به "در تعمیر" تغییر می‌دهیم
        if (!Log.IsResolved && Log.Type == MaintenanceType.Repair)
        {
            var asset = await _context.Assets.FindAsync(Log.AssetId);
            if (asset != null && asset.Status == AssetStatus.Active)
                asset.Status = AssetStatus.UnderRepair;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "تعمیر با موفقیت ثبت شد.";

        if (Log.AssetId > 0)
            return RedirectToPage("/Assets/Details", new { id = Log.AssetId });
        return RedirectToPage("/Maintenance/Index");
    }

    private async Task LoadSelectListAsync()
    {
        AssetList = new SelectList(
            await _context.Assets.OrderBy(a => a.Name)
                .Select(a => new { a.Id, Name = a.Name + (a.PropertyTag != null ? " (" + a.PropertyTag + ")" : "") })
                .ToListAsync(), "Id", "Name");
    }
}
