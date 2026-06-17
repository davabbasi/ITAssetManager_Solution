using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assignments;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public CreateModel(ApplicationDbContext context) => _context = context;

    [BindProperty] public AssetAssignment Assignment { get; set; } = new();
    public Asset? Asset { get; set; }
    public SelectList AssetList { get; set; } = null!;
    public SelectList EmployeeList { get; set; } = null!;
    public SelectList DepartmentList { get; set; } = null!;

    public async Task OnGetAsync(int? assetId)
    {
        if (assetId.HasValue)
        {
            Asset = await _context.Assets
                .Include(a => a.Employee)
                .Include(a => a.Department)
                .FirstOrDefaultAsync(a => a.Id == assetId);

            if (Asset != null)
            {
                Assignment.AssetId = Asset.Id;
                Assignment.FromEmployeeId = Asset.EmployeeId;
                Assignment.FromDepartmentId = Asset.DepartmentId;
                Assignment.FromLocation = Asset.Location;
            }
        }
        await LoadSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Assignment.Asset");
        ModelState.Remove("Assignment.FromEmployee");
        ModelState.Remove("Assignment.ToEmployee");
        ModelState.Remove("Assignment.FromDepartment");
        ModelState.Remove("Assignment.ToDepartment");

        if (!ModelState.IsValid)
        {
            if (Assignment.AssetId > 0)
                Asset = await _context.Assets.Include(a => a.Employee).Include(a => a.Department)
                    .FirstOrDefaultAsync(a => a.Id == Assignment.AssetId);
            await LoadSelectListsAsync();
            return Page();
        }

        Assignment.AssignedAt = DateTime.Now;
        Assignment.AssignedBy = User.Identity?.Name;
        _context.AssetAssignments.Add(Assignment);

        // آپدیت تجهیز با اطلاعات جدید
        var asset = await _context.Assets.FindAsync(Assignment.AssetId);
        if (asset != null)
        {
            asset.EmployeeId = Assignment.ToEmployeeId;
            asset.DepartmentId = Assignment.ToDepartmentId;
            asset.Location = Assignment.ToLocation;
            asset.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "جابجایی با موفقیت ثبت شد.";

        if (Assignment.AssetId > 0)
            return RedirectToPage("/Assets/Details", new { id = Assignment.AssetId });
        return RedirectToPage("/Assignments/Index");
    }

    private async Task LoadSelectListsAsync()
    {
        AssetList = new SelectList(
            await _context.Assets.Where(a => a.Status != AssetStatus.Scrapped)
                .OrderBy(a => a.Name)
                .Select(a => new { a.Id, Name = a.Name + (a.PropertyTag != null ? " (" + a.PropertyTag + ")" : "") })
                .ToListAsync(), "Id", "Name");

        EmployeeList = new SelectList(
            await _context.Employees.Where(e => e.IsActive)
                .Include(e => e.Department)
                .OrderBy(e => e.FullName)
                .Select(e => new { e.Id, Name = e.FullName + " - " + e.Department!.Name })
                .ToListAsync(), "Id", "Name");

        DepartmentList = new SelectList(
            await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(), "Id", "Name");
    }
}
