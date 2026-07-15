using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assets;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public EditModel(ApplicationDbContext context) => _context = context;

    [BindProperty] public Asset Asset { get; set; } = null!;
    public SelectList CategoryList { get; set; } = null!;
    public SelectList DepartmentList { get; set; } = null!;
    public SelectList EmployeeList { get; set; } = null!;
    public SelectList VendorList { get; set; } = null!;
    [BindProperty]
    public Dictionary<int, int> SpecValues { get; set; } = new();
    public List<Specification> CategorySpecifications { get; set; } = new();
    public async Task<IActionResult> OnGetAsync(int id)
    {
        Asset = await _context.Assets.FindAsync(id);
        if (Asset == null)
            return NotFound();
        await LoadSelectListsAsync();
        CategorySpecifications =
            await GetSpecificationsAsync(Asset.CategoryId);
        SpecValues = await _context.AssetSpecValues
            .Where(x => x.AssetId == Asset.Id)
            .ToDictionaryAsync(
                x => x.SpecDefinitionId,
                x => x.SpecValueId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Asset.Category");
        ModelState.Remove("Asset.Department");
        ModelState.Remove("Asset.Employee");

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        var oldSpecs = await _context.AssetSpecValues
            .Where(x => x.AssetId == Asset.Id)
            .ToListAsync();

        _context.AssetSpecValues.RemoveRange(oldSpecs);
        foreach (var (specId, valueId) in SpecValues)
        {
            if (valueId > 0)
            {
                _context.AssetSpecValues.Add(new AssetSpecificationValue
                {
                    AssetId = Asset.Id,
                    SpecDefinitionId = specId,
                    SpecValueId = valueId
                });
            }
        }


        await _context.SaveChangesAsync();

        TempData["Success"] = "تغییرات با موفقیت ذخیره شد.";
        return RedirectToPage("/Assets/Details", new { id = Asset.Id });
    }

    private async Task LoadSelectListsAsync()
    {
        CategoryList = new SelectList(
            await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        DepartmentList = new SelectList(
            await _context.VwDepartments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name");
        EmployeeList = new SelectList(
            await _context.VwEmployees

                .OrderBy(e => e.FullName)
                .Select(e => new { e.Id, Name = e.FullName + " - " + e.DepartmentName })
                .ToListAsync(), "Id", "Name");

        VendorList = new SelectList(
           await _context.Vendors
           .Where(v => v.IsActive)
           .OrderBy(v => v.Name)
           .ToListAsync(), "Id", "Name");
        CategorySpecifications = await GetSpecificationsAsync(Asset.CategoryId);
    }
    private async Task<List<Specification>> GetSpecificationsAsync(int categoryId)
    {
        return await _context.Specifications
            .Include(s => s.SpecValues)
            .Where(s => s.CategorySpecifications
                .Any(cs => cs.CategoryId == categoryId))
            .OrderBy(s => s.SortOrder)
            .ToListAsync();
    }

}
