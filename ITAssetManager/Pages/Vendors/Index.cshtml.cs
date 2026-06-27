using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Vendors;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public IndexModel(ApplicationDbContext context) => _context = context;

    public List<Vendor> Vendors { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        var query = _context.Vendors.AsQueryable();

        if (!string.IsNullOrEmpty(Search))
            query = query.Where(v =>
                v.Name.Contains(Search) ||
                v.Code.Contains(Search) ||
                (v.SalesPersonName != null && v.SalesPersonName.Contains(Search)) ||
                (v.Phone != null && v.Phone.Contains(Search)));

        Vendors = await query.OrderBy(v => v.Name).ToListAsync();
    }
}