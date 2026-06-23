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
                Asset = await _context.Assets
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

        // قبل از _context.AssetAssignments.Add(Assignment):
        if (Assignment.ToEmployeeId.HasValue)
        {
            var emp = await _context.VwEmployees.FindAsync(Assignment.ToEmployeeId);
            Assignment.ToEmployeeName = emp?.FullName;
        }
        if (Assignment.ToDepartmentId.HasValue)
        {
            var dept = await _context.VwDepartments.FindAsync(Assignment.ToDepartmentId);
            Assignment.ToDepartmentName = dept?.Name;
        }
        if (Assignment.FromEmployeeId.HasValue)
        {
            var emp = await _context.VwEmployees.FindAsync(Assignment.FromEmployeeId);
            Assignment.FromEmployeeName = emp?.FullName;
        }
        if (Assignment.FromDepartmentId.HasValue)
        {
            var dept = await _context.VwDepartments.FindAsync(Assignment.FromDepartmentId);
            Assignment.FromDepartmentName = dept?.Name;
        }

        // آپدیت Asset هم اسم رو ذخیره کن:
        if (asset != null)
        {
            asset.EmployeeId = Assignment.ToEmployeeId;
            asset.EmployeeName = Assignment.ToEmployeeName;
            asset.DepartmentId = Assignment.ToDepartmentId;
            asset.DepartmentName = Assignment.ToDepartmentName; // ← اضافه شد
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

        EmployeeList = new SelectList(await _context.VwEmployees
        .OrderBy(e => e.FullName)
        .Select(e => new { e.Id, Name = e.FullName + " - " + e.DepartmentName })
        .ToListAsync(), "Id", "Name");

        DepartmentList = new SelectList(await _context.VwDepartments
            .OrderBy(d => d.Name).ToListAsync(), "Id", "Name");
    }
}
