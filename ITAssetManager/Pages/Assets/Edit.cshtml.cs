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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();
        Asset = asset;
        await LoadSelectListsAsync();
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

        Asset.UpdatedAt = DateTime.Now;
        _context.Attach(Asset).State = EntityState.Modified;
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
    }
}
