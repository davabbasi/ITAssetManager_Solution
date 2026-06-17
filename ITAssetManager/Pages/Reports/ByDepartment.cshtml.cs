using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Reports;

[Authorize]
public class ByDepartmentModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public ByDepartmentModel(ApplicationDbContext context) => _context = context;

    public List<DeptGroup> DepartmentData { get; set; } = new();

    public async Task OnGetAsync()
    {
        var assets = await _context.Assets
            .Include(a => a.Category)
            .Include(a => a.Department)
            .Include(a => a.Employee)
            .Where(a => a.DepartmentId.HasValue)
            .OrderBy(a => a.Department!.Name)
            .ThenBy(a => a.Name)
            .ToListAsync();

        DepartmentData = assets
            .GroupBy(a => a.Department?.Name ?? "نامشخص")
            .Select(g => new DeptGroup { DepartmentName = g.Key, Assets = g.ToList() })
            .ToList();
    }

    public class DeptGroup
    {
        public string DepartmentName { get; set; } = "";
        public List<Asset> Assets { get; set; } = new();
    }
}
