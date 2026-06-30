using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Reports;

[Authorize]
public class ComponentTrackingModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public ComponentTrackingModel(ApplicationDbContext context) => _context = context;

    public List<ComponentTrackingItem> Items { get; set; } = new();
    public List<AssetCategory> InstalledCategories { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }

    public async Task OnGetAsync()
    {
        InstalledCategories = await _context.AssetCategories
            .Where(c => c.Type == AssetCategoryType.Installed)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var query = _context.Assets
            .Include(a => a.Category)
            .Where(a => a.Category!.Type == AssetCategoryType.Installed && !a.IsAssembled)
            .AsQueryable();

        if (!string.IsNullOrEmpty(Search))
            query = query.Where(a => a.Name.Contains(Search)
                || (a.SerialNumber != null && a.SerialNumber.Contains(Search))
                || (a.PropertyTag != null && a.PropertyTag.Contains(Search)));

        if (CategoryId.HasValue)
            query = query.Where(a => a.CategoryId == CategoryId);

        var assets = await query.OrderBy(a => a.Name).ToListAsync();
        var assetIds = assets.Select(a => a.Id).ToList();

        var activeAssemblies = await _context.AssemblyComponents
            .Include(c => c.PcAsset)
            .Where(c => assetIds.Contains(c.ComponentAssetId) && c.RemovedAt == null)
            .ToListAsync();

        Items = assets.Select(a => new ComponentTrackingItem
        {
            Asset = a,
            ActiveAssembly = activeAssemblies.FirstOrDefault(c => c.ComponentAssetId == a.Id)
        }).ToList();

        if (StatusFilter == "installed")
            Items = Items.Where(i => i.ActiveAssembly != null).ToList();
        else if (StatusFilter == "free")
            Items = Items.Where(i => i.ActiveAssembly == null).ToList();
    }

    public class ComponentTrackingItem
    {
        public Asset Asset { get; set; } = null!;
        public AssemblyComponent? ActiveAssembly { get; set; }
    }
}