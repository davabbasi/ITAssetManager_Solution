using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assets;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public CreateModel(ApplicationDbContext context) => _context = context;

    [BindProperty] public Asset Asset { get; set; } = new();

    public SelectList CategoryList { get; set; } = null!;
    public SelectList DepartmentList { get; set; } = null!;
    public SelectList EmployeeList { get; set; } = null!;
    public List<SpecDefinition> SpecDefinitions { get; set; } = new();
    public SelectList VendorList { get; set; } = null!;

    [BindProperty] public Dictionary<int, int> SpecValues { get; set; } = new();
    public async Task OnGetAsync()
    {
        if (Asset.CategoryId > 0)
            SpecDefinitions = await LoadSpecsAsync(Asset.CategoryId);
        await LoadSelectListsAsync();
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

        Asset.CreatedAt = DateTime.Now;
        _context.Assets.Add(Asset);
        await _context.SaveChangesAsync();

        // اگر تخصیص داده شده، یک رکورد جابجایی ثبت می‌کنیم
        if (Asset.EmployeeId.HasValue || Asset.DepartmentId.HasValue)
        {
            var assignment = new AssetAssignment
            {
                AssetId = Asset.Id,
                ToEmployeeId = Asset.EmployeeId,
                ToDepartmentId = Asset.DepartmentId,
                ToLocation = Asset.Location,
                AssignedAt = DateTime.Now,
                Reason = "ثبت اولیه",
                AssignedBy = User.Identity?.Name
            };
            _context.AssetAssignments.Add(assignment);
            await _context.SaveChangesAsync();
        }

        foreach (var (specDefId, specValId) in SpecValues)
        {
            if (specValId > 0)
            {
                _context.AssetSpecValues.Add(new AssetSpecValue
                {
                    AssetId = Asset.Id,
                    SpecDefinitionId = specDefId,
                    SpecValueId = specValId
                });
            }
        }
        await _context.SaveChangesAsync();


        TempData["Success"] = "تجهیز با موفقیت ثبت شد.";
        return RedirectToPage("/Assets/Details", new { id = Asset.Id });
    }

    private async Task LoadSelectListsAsync()
    {
        CategoryList = new SelectList(
            await _context.AssetCategories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");


        DepartmentList = new SelectList(await _context.VwDepartments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name");

        EmployeeList = new SelectList(
            await _context.VwEmployees
                .OrderBy(e => e.FullName)
                .Select(e => new { e.Id, Name = e.FullName + " - " + e.DepartmentName })
                .ToListAsync(), "Id", "Name");

        if (Asset.CategoryId > 0)
            SpecDefinitions = await LoadSpecsAsync(Asset.CategoryId);

        VendorList = new SelectList(
            await _context.Vendors
            .Where(v => v.IsActive)
            .OrderBy(v => v.Name)
            .ToListAsync(), "Id", "Name");
    }

    private async Task<List<SpecDefinition>> LoadSpecsAsync(int categoryId)
    {
        return await _context.SpecDefinitions
            .Include(s => s.SpecValues)
            .Where(s => s.CategoryId == categoryId)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();
    }


    public async Task<IActionResult> OnGetSpecsAsync(int categoryId)
    {
        var specs = await _context.SpecDefinitions
            .Include(x => x.SpecValues)
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync();

        return Partial("_SpecsPartial", specs);
    }

}
