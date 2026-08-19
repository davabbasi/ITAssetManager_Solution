using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assembly;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    // -----------------------------
    // اطلاعات اسمبل
    // -----------------------------

    public int NextAssemblyNumber { get; set; }

    [BindProperty]
    public DateTime AssembleDate { get; set; } = DateTime.Today;

    [BindProperty]
    public string PropertyTag { get; set; } = string.Empty;

    [BindProperty]
    public string? DeviceModel { get; set; }

    [BindProperty]
    public string? AssetName { get; set; }

    [BindProperty]
    public string? Description { get; set; }

    // قطعات انتخاب شده
    [BindProperty]
    public List<int> ComponentIds { get; set; } = new();

    // دسته‌بندی‌های قطعات
    public List<Category> InstalledCategories { get; set; } = new();


    // -----------------------------
    // GET
    // -----------------------------

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }


    // -----------------------------
    // POST
    // -----------------------------

    public async Task<IActionResult> OnPostAsync()
    {
        // --------------------------------
        // 1. اعتبارسنجی اولیه فرم
        // --------------------------------

        if (string.IsNullOrWhiteSpace(PropertyTag))
        {
            ModelState.AddModelError(
                nameof(PropertyTag),
                "شماره اموال الزامی است."
            );
        }

        if (ComponentIds == null || !ComponentIds.Any())
        {
            ModelState.AddModelError(
                "",
                "حداقل یک قطعه برای اسمبل باید انتخاب شود."
            );
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }


        // --------------------------------
        // 2. پیدا کردن دسته‌بندی PC
        // --------------------------------

        var pcCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == "کامپیوتر رومیزی");

        if (pcCategory == null)
        {
            ModelState.AddModelError(
                "",
                "دسته‌بندی «کامپیوتر رومیزی» در سیستم یافت نشد."
            );

            await LoadAsync();
            return Page();
        }


        // --------------------------------
        // 3. حذف آیتم‌های تکراری
        // --------------------------------

        var componentIds = ComponentIds
            .Distinct()
            .ToList();


        // --------------------------------
        // 4. پیدا کردن قطعات انتخاب شده
        // --------------------------------

        var components = await _context.Assets
            .Where(a => componentIds.Contains(a.Id))
            .ToListAsync();


        // --------------------------------
        // 5. بررسی تعداد قطعات
        // --------------------------------

        if (components.Count != componentIds.Count)
        {
            ModelState.AddModelError(
                "",
                "یکی از تجهیزات انتخاب‌شده دیگر وجود ندارد."
            );

            await LoadAsync();
            return Page();
        }


        // --------------------------------
        // 6. بررسی آزاد بودن قطعات
        // --------------------------------

        var busyComponents = await _context.AssemblyComponents
            .Where(ac =>
                componentIds.Contains(ac.ComponentAssetId) &&
                ac.RemovedAt == null)
            .Select(ac => ac.ComponentAssetId)
            .ToListAsync();

        if (busyComponents.Any())
        {
            ModelState.AddModelError(
                "",
                "یکی از قطعات انتخاب‌شده قبلاً در یک اسمبل دیگر استفاده شده است."
            );

            await LoadAsync();
            return Page();
        }


        // --------------------------------
        // 7. بررسی وضعیت قطعات
        // --------------------------------

        var invalidComponents = components
            .Where(a => a.Status != AssetStatus.InStorage)
            .ToList();

        if (invalidComponents.Any())
        {
            ModelState.AddModelError(
                "",
                "فقط تجهیزاتی که در انبار تجهیزات هستند می‌توانند در اسمبل استفاده شوند."
            );

            await LoadAsync();
            return Page();
        }


        // --------------------------------
        // 8. تعیین شماره اسمبل
        // --------------------------------

        var lastAssemblyNumber = await _context.Assets
            .Where(a => a.IsAssembled)
            .MaxAsync(a => (int?)a.AssemblyNumber) ?? 0;

        var assemblyNumber = lastAssemblyNumber + 1;


        // --------------------------------
        // 9. شروع Transaction
        // --------------------------------

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // --------------------------------
            // 10. ایجاد Asset برای PC اسمبل شده
            // --------------------------------

            var pcAsset = new Asset
            {
                Name = string.IsNullOrWhiteSpace(AssetName)
                    ? $"PC اسمبل‌شده #{assemblyNumber}"
                    : AssetName,

                Model = DeviceModel,

                PropertyTag = PropertyTag,

                CategoryId = pcCategory.Id,

                Status = AssetStatus.InStorage,

                StatusNote = "سیستم اسمبل‌شده و آماده تحویل",

                Notes = Description,

                IsAssembled = true,

                AssemblyNumber = assemblyNumber,

                CreatedAt = AssembleDate,

            };

            _context.Assets.Add(pcAsset);

            await _context.SaveChangesAsync();


            // --------------------------------
            // 11. اتصال قطعات به PC
            // --------------------------------

            var rowNumber = 1;

            foreach (var component in components)
            {
                _context.AssemblyComponents.Add(
                    new AssemblyComponent
                    {
                        PcAssetId = pcAsset.Id,

                        ComponentAssetId = component.Id,

                        InstalledAt = DateTime.Now,

                        InstalledBy = User.Identity?.Name,

                        // اگر در مدل AssemblyComponent داری
                        // RowNumber = rowNumber++
                    });
            }


            // --------------------------------
            // 12. تغییر وضعیت قطعات
            // --------------------------------

            foreach (var component in components)
            {
                component.Status = AssetStatus.Active;

                component.UpdatedAt = DateTime.Now;
            }


            // --------------------------------
            // 13. ذخیره نهایی
            // --------------------------------

            await _context.SaveChangesAsync();


            // --------------------------------
            // 14. ثبت Transaction
            // --------------------------------

            await transaction.CommitAsync();


            // --------------------------------
            // 15. پیام موفقیت
            // --------------------------------

            TempData["Success"] =
                $"سیستم شماره {assemblyNumber} با موفقیت اسمبل شد.";


            return RedirectToPage(
                "/Assets/Details",
                new { id = pcAsset.Id }
            );
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                "",
                "در هنگام ایجاد اسمبل خطایی رخ داد. هیچ تغییری ثبت نشد."
            );

            await LoadAsync();

            return Page();
        }
    }


    // -----------------------------
    // Load Lists
    // -----------------------------

    private async Task LoadAsync()
    {
        // --------------------------------
        // شماره اسمبل بعدی
        // --------------------------------

        var lastNumber = await _context.Assets
            .Where(a => a.IsAssembled)
            .MaxAsync(a => (int?)a.AssemblyNumber) ?? 0;

        NextAssemblyNumber = lastNumber + 1;


        // --------------------------------
        // دسته‌بندی قطعات
        // --------------------------------

        InstalledCategories = await _context.Categories
            .Where(c => c.Type == AssetCategoryType.Installed)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}