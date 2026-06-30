using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.API;

[Authorize]
public class FreeComponentsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public FreeComponentsModel(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> OnGetAsync(int categoryId)
    {
        // قطعاتی که: دسته‌بندی نصبیه + الان توی هیچ PC فعالی نیستن + خودشون PC نیستن
        var usedComponentIds = await _context.AssemblyComponents
            .Where(ac => ac.RemovedAt == null)
            .Select(ac => ac.ComponentAssetId)
            .ToListAsync();

        var items = await _context.Assets
            .Include(a => a.Category)
            .Where(a => a.CategoryId == categoryId
                && !a.IsAssembled
                && a.Status != AssetStatus.Scrapped
                && !usedComponentIds.Contains(a.Id))
            .OrderBy(a => a.Name)
            .Select(a => new
            {
                id = a.Id,
                name = a.Name,
                propertyTag = a.PropertyTag,
                serialNumber = a.SerialNumber,
                categoryName = a.Category!.Name
            })
            .ToListAsync();

        return new JsonResult(items);
    }
}