using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Maintenance;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public IndexModel(ApplicationDbContext context) => _context = context;

    public List<MaintenanceLog> Logs { get; set; } = new();
    public int OpenCount { get; set; }
    public int ResolvedCount { get; set; }
    public decimal TotalCost { get; set; }

    public async Task OnGetAsync()
    {
        Logs = await _context.MaintenanceLogs
            .Include(m => m.Asset)
            .OrderByDescending(m => m.StartDate)
            .ToListAsync();

        OpenCount = Logs.Count(m => !m.IsResolved);
        ResolvedCount = Logs.Count(m => m.IsResolved);
        TotalCost = Logs.Where(m => m.Cost.HasValue).Sum(m => m.Cost!.Value);
    }
}
