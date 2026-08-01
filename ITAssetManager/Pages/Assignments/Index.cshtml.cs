using ITAssetManager.Convertor;
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
    [BindProperty(SupportsGet = true)] public string? FromDate { get; set; } = null;
    [BindProperty(SupportsGet = true)] public string? ToDate { get; set; } = null;

    public async Task OnGetAsync()
    {
        var query = _context.AssetAssignments
            .Include(a => a.Asset).Where(a=>a.Asset.Category.Type!=AssetCategoryType.Installed)
            .AsQueryable();

        if (!string.IsNullOrEmpty(Search))
            query = query.Where(a =>
                (a.Asset != null && a.Asset.Name.Contains(Search)) ||
                (a.ToEmployeeName != null && a.ToEmployeeName.Contains(Search)) ||
                (a.FromEmployeeName != null && a.FromEmployeeName.Contains(Search)));


        DateTime? fromDate = string.IsNullOrWhiteSpace(FromDate)? null: FromDate.ToMiladi();
        DateTime? toDate = string.IsNullOrWhiteSpace(ToDate)? null: ToDate.ToMiladi();

        if (fromDate != null)
            query = query.Where(a => a.AssignedAt >= fromDate);
        if (toDate != null)
            query = query.Where(a => a.AssignedAt <= toDate.Value.AddDays(1));

        Assignments = await query.OrderByDescending(a => a.AssignedAt).ToListAsync();
    }
}
