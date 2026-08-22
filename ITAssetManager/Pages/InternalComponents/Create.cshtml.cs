using System.ComponentModel.DataAnnotations;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
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
    [Required(ErrorMessage = "شماره اموال الزامی است.")]
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


    public async Task OnGetAsync()
    {
        await LoadAsync();
    }


    public async Task<IActionResult> OnPostAsync()
    {
        // =========================================================
        // 1. اعتبارسنجی اولیه فرم
        // =========================================================

        if (string.IsNullOrWhiteSpace(PropertyTag))
        {
            ModelState.AddModelError(
                nameof(PropertyTag),
                "شماره اموال الزامی است.");

            await LoadAsync();
            return Page();
        }

        if (ComponentIds == null || !ComponentIds.Any())
        {
            ModelState.AddModelError(
                nameof(ComponentIds),
                "حداقل یک قطعه باید برای اسمبل انتخاب شود.");

            await LoadAsync();
            return Page();
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }


        // =========================================================
        // 2. حذف IDهای تکراری قطعات
        // =========================================================

        var componentIds = ComponentIds
            .Distinct()
            .ToList();


        // =========================================================
        // 3. پیدا کردن قطعات انتخاب‌شده
        // =========================================================

        var components = await _context.Assets
            .Include(a => a.Category)
            .Include(a => a.Product)
            .Include(a => a.Warehouse)
            .Where(a => componentIds.Contains(a.Id))
            .ToListAsync();


        // اگر تعداد قطعات پیدا شده با تعداد انتخاب شده یکی نباشد
        if (components.Count != componentIds.Count)
        {
            ModelState.AddModelError(
                "",
                "یکی از قطعات انتخاب‌شده در سیستم پیدا نشد.");

            await LoadAsync();
            return Page();
        }


        // =========================================================
        // 4. بررسی وضعیت تک تک قطعات
        //    قطعه باید در وضعیت InStorage باشد
        // =========================================================

        foreach (var component in components)
        {
            if (component.Status != AssetStatus.InStorage)
            {
                ModelState.AddModelError(
                    "",
                    $"قطعه «{component.Name}» در انبار تجهیزات نیست.");

                await LoadAsync();
                return Page();
            }
        }


        // =========================================================
        // 5. بررسی اینکه هر قطعه انبار داشته باشد
        // =========================================================

        foreach (var component in components)
        {
            if (component.WarehouseId <= 0)
            {
                ModelState.AddModelError(
                    "",
                    $"برای قطعه «{component.Name}» انبار مشخص نشده است.");

                await LoadAsync();
                return Page();
            }
        }


        // =========================================================
        // 6. پیدا کردن انبار تجهیزات
        // =========================================================

        var assetWarehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.IsAssetWarehouse);

        if (assetWarehouse == null)
        {
            ModelState.AddModelError(
                "",
                "انبار تجهیزات در سیستم تعریف نشده است.");

            await LoadAsync();
            return Page();
        }


        // =========================================================
        // 7. بررسی اینکه قطعه‌ای قبلاً داخل اسمبل دیگری نباشد
        // =========================================================

        var alreadyInstalledIds = await _context.AssemblyComponents
            .Where(ac =>
                componentIds.Contains(ac.ComponentAssetId) &&
                ac.RemovedAt == null)
            .Select(ac => ac.ComponentAssetId)
            .ToListAsync();

        if (alreadyInstalledIds.Any())
        {
            var installedAssets = components
                .Where(c => alreadyInstalledIds.Contains(c.Id))
                .Select(c => c.Name)
                .ToList();

            ModelState.AddModelError(
                "",
                $"قطعه‌های زیر قبلاً داخل یک اسمبل فعال هستند: {string.Join("، ", installedAssets)}");

            await LoadAsync();
            return Page();
        }


        // =========================================================
        // 8. بررسی موجودی کالاهای قطعات در انبارهای مربوطه
        // =========================================================

        foreach (var component in components)
        {
            var stock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(s =>
                    s.WarehouseId == component.WarehouseId &&
                    s.ProductId == component.ProductId);

            if (stock == null || stock.Quantity < 1)
            {
                ModelState.AddModelError(
                    "",
                    $"موجودی کالای «{component.Product?.ProductName ?? component.Name}» " +
                    $"در انبار مربوطه کافی نیست.");

                await LoadAsync();
                return Page();
            }
        }


        // =========================================================
        // 9. پیدا کردن دسته‌بندی کامپیوتر رومیزی
        // =========================================================

        var pcCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == "کامپیوتر رومیزی");

        if (pcCategory == null)
        {
            ModelState.AddModelError(
                "",
                "دسته‌بندی «کامپیوتر رومیزی» در سیستم پیدا نشد.");

            await LoadAsync();
            return Page();
        }


        // =========================================================
        // 10. شروع Transaction
        // =========================================================

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // =====================================================
            // 11. تولید شماره اسمبل
            // =====================================================

            var lastAssemblyNumber =
                await _context.Assets
                    .Where(a => a.IsAssembled)
                    .MaxAsync(a => (int?)a.AssemblyNumber) ?? 0;

            var assemblyNumber = lastAssemblyNumber + 1;


            // =====================================================
            // 12. ایجاد Asset مربوط به PC اسمبل‌شده
            // =====================================================

            var pcAsset = new Asset
            {
                Name = string.IsNullOrWhiteSpace(AssetName)
                    ? $"PC اسمبل‌شده #{assemblyNumber}"
                    : AssetName,

                Model = DeviceModel,

                PropertyTag = PropertyTag,

                CategoryId = pcCategory.Id,

                ProductId = 8,

                WarehouseId = assetWarehouse.Id,

                Status = AssetStatus.InStorage,

                Notes = Description,

                IsAssembled = true,

                AssemblyNumber = assemblyNumber,

                CreatedAt = AssembleDate
            };

            _context.Assets.Add(pcAsset);

            await _context.SaveChangesAsync();


            // =====================================================
            // 13. ایجاد AssemblyComponent برای هر قطعه
            // =====================================================

            foreach (var component in components)
            {
                _context.AssemblyComponents.Add(
                    new AssemblyComponent
                    {
                        PcAssetId = pcAsset.Id,

                        ComponentAssetId = component.Id,

                        InstalledAt = DateTime.Now,

                        InstalledBy = User.Identity?.Name,

                        Notes = $"استفاده در اسمبل شماره {assemblyNumber}"
                    });
            }


            // =====================================================
            // 14. گروه‌بندی قطعات بر اساس انبار
            // =====================================================

            var componentsByWarehouse = components
                .GroupBy(c => c.WarehouseId)
                .ToList();


            // =====================================================
            // 15. ایجاد حواله خروج برای هر انبار
            // =====================================================

            foreach (var warehouseGroup in componentsByWarehouse)
            {
                var warehouseId = warehouseGroup.Key;


                // -----------------------------------------------
                // شماره حواله بعدی
                // -----------------------------------------------

                var lastIssueNumber =
                    await _context.WarehouseIssues
                        .MaxAsync(x => (int?)x.IssueNumber) ?? 0;

                var issueNumber = lastIssueNumber + 1;


                // -----------------------------------------------
                // ایجاد حواله
                // -----------------------------------------------

                var issue = new WarehouseIssue
                {
                    IssueNumber = issueNumber,

                    IssueDate = DateTime.Now,

                    CreatedAt = DateTime.Now,

                    CreatedBy = User.Identity?.Name,

                    WarehouseId = warehouseId,

                    Status = DocumentStatus.Posted,

                    Description =
                        $"حواله خروج قطعات جهت اسمبل سیستم شماره {assemblyNumber}"
                };

                _context.WarehouseIssues.Add(issue);

                await _context.SaveChangesAsync();


                // -----------------------------------------------
                // قطعات این حواله
                // -----------------------------------------------

                var rowNumber = 1;

                foreach (var component in warehouseGroup)
                {
                    var issueItem = new WarehouseIssueItem
                    {
                        IssueId = issue.Id,

                        RowNumber = rowNumber++,

                        ProductId = (int)component.ProductId,

                        Quantity = 1,

                        Description =
                            $"قطعه {component.Name} - اسمبل #{assemblyNumber}"
                    };

                    _context.WarehouseIssueItems.Add(issueItem);

                    await _context.SaveChangesAsync();


                    // -------------------------------------------
                    // کاهش موجودی کالا
                    // -------------------------------------------

                    var stock = await _context.WarehouseStocks
                        .FirstAsync(s =>
                            s.WarehouseId == warehouseId &&
                            s.ProductId == component.ProductId);

                    stock.Quantity -= 1;
                    stock.UpdatedAt = DateTime.Now;


                    // -------------------------------------------
                    // ثبت تراکنش خروج
                    // -------------------------------------------

                    _context.InventoryTransactions.Add(
                        new InventoryTransaction
                        {
                            WarehouseId = warehouseId,

                            ProductId = (int)component.ProductId,

                            Quantity = -1,

                            Type = InventoryTransactionType.Issue,

                            TransactionDate = DateTime.Now,

                            IssueItemId = issueItem.Id,

                            Description =
                                $"خروج قطعه جهت اسمبل سیستم #{assemblyNumber}",

                            CreatedAt = DateTime.Now,

                            CreatedBy = User.Identity?.Name ?? "System"
                        });
                }
            }


            // =====================================================
            // 16. تغییر وضعیت قطعات
            // =====================================================

            foreach (var component in components)
            {
                component.Status = AssetStatus.Active;

                component.WarehouseId = assetWarehouse.Id;
            }


            // =====================================================
            // 17. افزایش موجودی PC اسمبل‌شده
            // =====================================================

            var pcStock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(s =>
                    s.WarehouseId == assetWarehouse.Id &&
                    s.ProductId == 8);

            if (pcStock == null)
            {
                pcStock = new WarehouseStock
                {
                    WarehouseId = assetWarehouse.Id,

                    ProductId = 8,

                    Quantity = 1,

                    UpdatedAt = DateTime.Now
                };

                _context.WarehouseStocks.Add(pcStock);
            }
            else
            {
                pcStock.Quantity += 1;
                pcStock.UpdatedAt = DateTime.Now;
            }


            // =====================================================
            // 18. ثبت تراکنش ورود PC اسمبل‌شده
            // =====================================================

            _context.InventoryTransactions.Add(
                new InventoryTransaction
                {
                    WarehouseId = assetWarehouse.Id,

                    ProductId = 8,

                    Quantity = 1,

                    Type = InventoryTransactionType.AssemblyIn,

                    TransactionDate = DateTime.Now,

                    AssetId = pcAsset.Id,

                    Description =
                        $"ورود سیستم اسمبل‌شده #{assemblyNumber} به انبار تجهیزات",

                    CreatedAt = DateTime.Now,

                    CreatedBy = User.Identity?.Name ?? "System"
                });


            // =====================================================
            // 19. ذخیره تمام تغییرات
            // =====================================================

            await _context.SaveChangesAsync();


            // =====================================================
            // 20. Commit
            // =====================================================

            await transaction.CommitAsync();


            // =====================================================
            // 21. پیام موفقیت
            // =====================================================

            TempData["Success"] =
                $"سیستم اسمبل‌شده با شماره #{assemblyNumber} با موفقیت ایجاد شد.";


            // =====================================================
            // 22. انتقال به صفحه جزئیات Asset
            // =====================================================

            return RedirectToPage(
                "/Assets/Details",
                new { id = pcAsset.Id });
        }
        catch (Exception)
        {
            // =====================================================
            // در صورت خطا همه تغییرات Rollback می‌شوند
            // =====================================================

            await transaction.RollbackAsync();

            ModelState.AddModelError(
                "",
                "در هنگام ثبت اسمبل خطایی رخ داد. هیچ تغییری در سیستم اعمال نشد.");

            await LoadAsync();
            return Page();
        }
    }
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