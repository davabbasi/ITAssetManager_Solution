using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assembly;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public CreateModel(ApplicationDbContext context) => _context = context;

    public int NextAssemblyNumber { get; set; }
    public List<Category> InstalledCategories { get; set; } = new();

    [BindProperty] public DateTime AssembleDate { get; set; } = DateTime.Today;
    [BindProperty] public string PropertyTag { get; set; } = string.Empty;
    [BindProperty] public string? DeviceModel { get; set; }
    [BindProperty] public string? AssetName { get; set; }
    [BindProperty] public string? Description { get; set; }
    [BindProperty] public List<int> ComponentIds { get; set; } = new();
    [BindProperty] public int? DepartmentId { get; set; }
    [BindProperty] public int? EmployeeId { get; set; }
    public SelectList DepartmentList { get; set; } = null!;
    public SelectList EmployeeList { get; set; } = null!;

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(PropertyTag) || ComponentIds == null || !ComponentIds.Any())
        {
            ModelState.AddModelError("", "شماره اموال و حداقل یک قطعه الزامی است.");
            await LoadAsync();
            return Page();
        }

        // دسته‌بندی "کامپیوتر رومیزی"
        var pcCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == "کامپیوتر رومیزی");
        if (pcCategory == null)
        {
            ModelState.AddModelError("", "دسته‌بندی «کامپیوتر رومیزی» در سیستم یافت نشد.");
            await LoadAsync();
            return Page();
        }

        // شماره اسمبل بعدی
        var lastNumber = await _context.Assets
            .Where(a => a.IsAssembled)
            .MaxAsync(a => (int?)a.AssemblyNumber) ?? 0;

        // ساخت Asset برای خود PC
        var pcAsset = new Asset
        {
            Name = string.IsNullOrWhiteSpace(AssetName) ? $"PC اسمبل‌شده #{lastNumber + 1}" : AssetName,
            Model = DeviceModel,
            PropertyTag = PropertyTag,
            CategoryId = pcCategory.Id,
            Status = AssetStatus.Active,
            Notes = Description,
            IsAssembled = true,
            AssemblyNumber = lastNumber + 1,
            CreatedAt = AssembleDate,
            DepartmentId = DepartmentId,
            EmployeeId = EmployeeId
        };
        _context.Assets.Add(pcAsset);
        await _context.SaveChangesAsync();

        // پر کردن نام‌های واحد و کارمند (چون از View می‌خونیم، FK نداریم)
        if (DepartmentId.HasValue)
        {
            var dept = await _context.VwDepartments.FindAsync(DepartmentId);
            pcAsset.DepartmentName = dept?.Name;
        }
        if (EmployeeId.HasValue)
        {
            var emp = await _context.VwEmployees.FindAsync(EmployeeId);
            pcAsset.EmployeeName = emp?.FullName;
        }
        await _context.SaveChangesAsync();

        // اتصال قطعات انتخاب‌شده
        foreach (var compId in ComponentIds.Distinct())
        {
            _context.AssemblyComponents.Add(new AssemblyComponent
            {
                PcAssetId = pcAsset.Id,
                ComponentAssetId = compId,
                InstalledAt = DateTime.Now,
                InstalledBy = User.Identity?.Name
            });
        }
        await _context.SaveChangesAsync();

        // ثبت رکورد جابجایی اولیه (مثل بقیه تجهیزات)
        if (DepartmentId.HasValue || EmployeeId.HasValue)
        {
            _context.AssetAssignments.Add(new AssetAssignment
            {
                AssetId = pcAsset.Id,
                ToEmployeeId = EmployeeId,
                ToEmployeeName = pcAsset.EmployeeName,
                ToDepartmentId = DepartmentId,
                ToDepartmentName = pcAsset.DepartmentName,
                AssignedAt = DateTime.Now,
                Reason = "اسمبل اولیه سیستم",
                AssignedBy = User.Identity?.Name
            });
            await _context.SaveChangesAsync();
        }

        TempData["Success"] = $"سیستم #{pcAsset.AssemblyNumber} با موفقیت اسمبل شد.";
        return RedirectToPage("/Assembly/Details", new { id = pcAsset.Id });
    }

    private async Task LoadAsync()
    {
        var lastNumber = await _context.Assets
            .Where(a => a.IsAssembled)
            .MaxAsync(a => (int?)a.AssemblyNumber) ?? 0;
        NextAssemblyNumber = lastNumber + 1;

        // دسته‌بندی‌های نصبی
        InstalledCategories = await _context.Categories
            .Where(c => c.Type == AssetCategoryType.Installed)
            .OrderBy(c => c.Name)
            .ToListAsync();
        DepartmentList = new SelectList(
            await _context.VwDepartments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name");
        EmployeeList = new SelectList(
            await _context.VwEmployees
                .OrderBy(e => e.FullName)
                .Select(e => new { e.Id, Name = e.FullName + " - " + e.DepartmentName })
                .ToListAsync(), "Id", "Name");
    }
}