using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assembly;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public IndexModel(ApplicationDbContext context) => _context = context;

    public List<Asset> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        Items = await _context.Assets
            .Include(a => a.Components)
            .Where(a => a.IsAssembled)
            .OrderByDescending(a => a.AssemblyNumber)
            .ToListAsync();
    }
}