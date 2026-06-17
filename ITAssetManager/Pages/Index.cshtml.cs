using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public IndexModel(ApplicationDbContext context) => _context = context;

    public int TotalAssets { get; set; }
    public int ActiveAssets { get; set; }
    public int UnderRepair { get; set; }
    public int WarrantyExpiringSoon { get; set; }

    public List<Asset> RecentAssets { get; set; } = new();
    public List<Asset> ExpiringWarrantyAssets { get; set; } = new();
    public List<CategoryCount> AssetsByCategory { get; set; } = new();

    public async Task OnGetAsync()
    {
        var assets = _context.Assets.Include(a => a.Category)
                                    .Include(a => a.Department)
                                    .Include(a => a.Employee);

        TotalAssets = await assets.CountAsync();
        ActiveAssets = await assets.CountAsync(a => a.Status == AssetStatus.Active);
        UnderRepair = await assets.CountAsync(a => a.Status == AssetStatus.UnderRepair);

        var in60Days = DateTime.Today.AddDays(60);
        WarrantyExpiringSoon = await assets.CountAsync(a =>
            a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value <= in60Days);

        RecentAssets = await assets.OrderByDescending(a => a.CreatedAt).Take(8).ToListAsync();

        ExpiringWarrantyAssets = await assets
            .Where(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value <= in60Days)
            .OrderBy(a => a.WarrantyExpiry)
            .Take(5).ToListAsync();

        AssetsByCategory = await _context.Assets
            .Include(a => a.Category)
            .GroupBy(a => a.Category!.Name)
            .Select(g => new CategoryCount { CategoryName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(6).ToListAsync();
    }

    public class CategoryCount
    {
        public string CategoryName { get; set; } = "";
        public int Count { get; set; }
    }
}
