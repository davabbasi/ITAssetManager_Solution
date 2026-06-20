using ITAssetManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Api;

[Authorize]
public class SpecsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public SpecsModel(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> OnGetAsync(int categoryId)
    {
        var specs = await _context.SpecDefinitions
            .Include(s => s.SpecValues)
            .Where(s => s.CategoryId == categoryId)
            .OrderBy(s => s.SortOrder)
            .Select(s => new
            {
                id = s.Id,
                name = s.Name,
                values = s.SpecValues.OrderBy(v => v.SortOrder)
                    .Select(v => new { id = v.Id, value = v.Value })
            })
            .ToListAsync();

        return new JsonResult(specs);
    }
}