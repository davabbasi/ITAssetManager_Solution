using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Reports;

[Authorize]
public class WarrantyModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public WarrantyModel(ApplicationDbContext context) => _context = context;

    public int Expired { get; set; }
    public int ExpiringSoon30 { get; set; }
    public int ExpiringSoon90 { get; set; }
    public int Valid { get; set; }
    public List<Asset> ExpiredAssets { get; set; } = new();
    public List<Asset> ExpiringSoonAssets { get; set; } = new();

    public async Task OnGetAsync()
    {
        var today = DateTime.Today;
        var in90 = today.AddDays(90);
        var in30 = today.AddDays(30);

        var assets = await _context.Assets
            .Where(a => a.WarrantyExpiry.HasValue && a.Status != AssetStatus.Scrapped)
            .ToListAsync();

        ExpiredAssets = assets.Where(a => a.WarrantyExpiry!.Value < today)
            .OrderBy(a => a.WarrantyExpiry).ToList();

        ExpiringSoonAssets = assets.Where(a => a.WarrantyExpiry!.Value >= today && a.WarrantyExpiry.Value <= in90)
            .OrderBy(a => a.WarrantyExpiry).ToList();

        Expired = ExpiredAssets.Count;
        ExpiringSoon30 = assets.Count(a => a.WarrantyExpiry!.Value >= today && a.WarrantyExpiry.Value <= in30);
        ExpiringSoon90 = ExpiringSoonAssets.Count;
        Valid = assets.Count(a => a.WarrantyExpiry!.Value > in90);
    }
}
