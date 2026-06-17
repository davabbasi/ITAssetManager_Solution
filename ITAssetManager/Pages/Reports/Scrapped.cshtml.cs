using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Reports;

[Authorize]
public class ScrappedModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public ScrappedModel(ApplicationDbContext context) => _context = context;

    public List<Asset> Scrapped { get; set; } = new();
    public List<Asset> Faulty { get; set; } = new();
    public List<Asset> UnderRepair { get; set; } = new();

    public async Task OnGetAsync()
    {
        var assets = await _context.Assets
            .Include(a => a.Category)
            .Include(a => a.Department)
            .Include(a => a.Employee)
            .Where(a => a.Status == AssetStatus.Scrapped ||
                        a.Status == AssetStatus.Faulty ||
                        a.Status == AssetStatus.UnderRepair)
            .OrderBy(a => a.Name)
            .ToListAsync();

        Scrapped = assets.Where(a => a.Status == AssetStatus.Scrapped).ToList();
        Faulty = assets.Where(a => a.Status == AssetStatus.Faulty).ToList();
        UnderRepair = assets.Where(a => a.Status == AssetStatus.UnderRepair).ToList();
    }
}
