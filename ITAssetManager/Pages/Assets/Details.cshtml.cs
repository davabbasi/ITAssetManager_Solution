using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assets;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public DetailsModel(ApplicationDbContext context) => _context = context;

    public Asset Asset { get; set; } = null!;
    public List<AssetAssignment> Assignments { get; set; } = new();
    public List<MaintenanceLog> MaintenanceLogs { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var asset = await _context.Assets
            .Include(a => a.Category)          
            .Include(a => a.SpecValues)
            .ThenInclude(sv => sv.Specification)
            .Include(a => a.SpecValues)
            .ThenInclude(sv => sv.SpecValue)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null) return NotFound();
        Asset = asset;

        Assignments = await _context.AssetAssignments
            .Where(a => a.AssetId == id)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

        MaintenanceLogs = await _context.MaintenanceLogs
            .Where(m => m.AssetId == id)
            .OrderByDescending(m => m.StartDate)
            .ToListAsync();

        return Page();
    }
}
