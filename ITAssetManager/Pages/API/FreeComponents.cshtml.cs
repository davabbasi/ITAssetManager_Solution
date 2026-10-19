
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.API;

[Authorize(Policy = "RequireITRole")]
public class FreeComponentsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public FreeComponentsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> OnGetAsync(
        int warehouseId,
        int categoryId)
    {
        if (warehouseId <= 0 || categoryId <= 0)
            return BadRequest();

        // اطمینان از اینکه انبار انتخاب‌شده، انبار تجهیزات است.
        var warehouseExists = await _context.Warehouses
            .AnyAsync(w =>
                w.Id == warehouseId &&
                w.IsAssetWarehouse);

        if (!warehouseExists)
            return BadRequest("انبار انتخاب‌شده معتبر نیست.");

        // قطعاتی که در حال حاضر در یک اسمبل فعال هستند.
        var usedComponentIds = _context.AssemblyComponents
            .Where(ac => ac.RemovedAt == null)
            .Select(ac => ac.ComponentAssetId);

        // قطعات آزاد متعلق به انبار انتخاب‌شده.
        var items = await _context.Assets
            .Where(a =>
                a.CategoryId == categoryId &&
                a.WarehouseId == warehouseId &&
                !a.IsAssembled &&
                a.Status == AssetStatus.InStorage &&
                !usedComponentIds.Contains(a.Id))
            .OrderBy(a => a.Name)
            .Select(a => new
            {
                id = a.Id,
                name = a.Name,
                propertyTag = a.PropertyTag,
                serialNumber = a.SerialNumber
            })
            .ToListAsync();

        return new JsonResult(items);
    }
}
