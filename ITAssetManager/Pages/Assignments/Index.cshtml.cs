using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assignments;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public IndexModel(ApplicationDbContext context) => _context = context;

    public List<AssetAssignment> Assignments { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }

    public async Task OnGetAsync()
    {
        var query = _context.AssetAssignments
            .Include(a => a.Asset)
            .Include(a => a.FromEmployee)
            .Include(a => a.ToEmployee)
            .Include(a => a.FromDepartment)
            .Include(a => a.ToDepartment)
            .AsQueryable();

        if (!string.IsNullOrEmpty(Search))
            query = query.Where(a =>
                (a.Asset != null && a.Asset.Name.Contains(Search)) ||
                (a.ToEmployee != null && a.ToEmployee.FullName.Contains(Search)) ||
                (a.FromEmployee != null && a.FromEmployee.FullName.Contains(Search)));

        if (FromDate.HasValue)
            query = query.Where(a => a.AssignedAt >= FromDate);
        if (ToDate.HasValue)
            query = query.Where(a => a.AssignedAt <= ToDate.Value.AddDays(1));

        Assignments = await query.OrderByDescending(a => a.AssignedAt).ToListAsync();
    }
}
