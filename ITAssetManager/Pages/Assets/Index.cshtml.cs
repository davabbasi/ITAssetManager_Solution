using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assets;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public IndexModel(ApplicationDbContext context) => _context = context;

    public List<Asset> Assets { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<VwDepartment> Departments { get; set; } = new();
    public int TotalCount { get; set; }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public int? Status { get; set; }
    [BindProperty(SupportsGet = true)] public int? DepartmentId { get; set; }

    public async Task OnGetAsync()
    {
        Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        Departments = await _context.VwDepartments.OrderBy(d => d.Name).ToListAsync();

        var query = _context.Assets
            .Include(a => a.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(Search))
            query = query.Where(a =>
                a.Name.Contains(Search) ||
                (a.SerialNumber != null && a.SerialNumber.Contains(Search)) ||
                (a.Barcode != null && a.Barcode.Contains(Search)) ||
                (a.PropertyTag != null && a.PropertyTag.Contains(Search)) ||
                (a.Model != null && a.Model.Contains(Search)));

        if (CategoryId.HasValue)
            query = query.Where(a => a.CategoryId == CategoryId);

        if (Status.HasValue)
            query = query.Where(a => (int)a.Status == Status);

        if (DepartmentId.HasValue)
            query = query.Where(a => a.DepartmentId == DepartmentId);

        TotalCount = await query.CountAsync();
        Assets = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }
}
