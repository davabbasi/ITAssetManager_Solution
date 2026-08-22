using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assembly;

[Authorize]
public class AddComponentModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public AddComponentModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Asset PcAsset { get; set; } = null!;

    public List<Category> InstalledCategories { get; set; } = new();


    // =========================================================
    // GET
    // =========================================================

    public async Task<IActionResult> OnGetAsync(int pcId)
    {
        // 1. پیدا کردن سیستم اسمبل‌شده

        var pc = await _context.Assets
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a =>
                a.Id == pcId &&
                a.IsAssembled &&
                a.Category != null &&
                a.Category.HasInternalComponent);

        if (pc == null)
            return NotFound();

        PcAsset = pc;


        // 2. دسته‌بندی قطعات قابل نصب

        InstalledCategories = await _context.Categories
            .Where(c => c.Type == AssetCategoryType.Installed)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Page();
    }


    // =========================================================
    // POST
    // =========================================================

    public async Task<IActionResult> OnPostAsync(int pcId,int componentId)
    {
        // =====================================================
        // 1. پیدا کردن PC
        // =====================================================

        var pc = await _context.Assets
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a =>
                a.Id == pcId &&
                a.IsAssembled &&
                a.Category != null &&
                a.Category.HasInternalComponent);

        if (pc == null)
            return NotFound();


        // =====================================================
        // 2. بررسی انتخاب قطعه
        // =====================================================

        if (componentId <= 0)
        {
            ModelState.AddModelError(
                "componentId",
                "لطفاً قطعه را انتخاب کنید.");

            return await ReturnPageAsync(pcId);
        }


        // =====================================================
        // 3. پیدا کردن قطعه
        // =====================================================

        var component = await _context.Assets
            .Include(a => a.Product)
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Id == componentId);

        if (component == null)
        {
            ModelState.AddModelError(
                "componentId",
                "قطعه موردنظر پیدا نشد.");

            return await ReturnPageAsync(pcId);
        }


        // =====================================================
        // 4. بررسی وضعیت قطعه
        // =====================================================

        if (component.Status != AssetStatus.InStorage)
        {
            ModelState.AddModelError(
                "",
                $"قطعه «{component.Name}» در انبار تجهیزات نیست.");

            return await ReturnPageAsync(pcId);
        }


        // =====================================================
        // 5. بررسی اینکه قطعه داخل اسمبل دیگری نباشد
        // =====================================================

        var alreadyInstalled =
            await _context.AssemblyComponents
                .AnyAsync(x =>
                    x.ComponentAssetId == componentId &&
                    x.RemovedAt == null);

        if (alreadyInstalled)
        {
            ModelState.AddModelError(
                "",
                $"قطعه «{component.Name}» در حال حاضر داخل یک اسمبل دیگر قرار دارد.");

            return await ReturnPageAsync(pcId);
        }


        // =====================================================
        // 6. پیدا کردن انبار تجهیزات
        // =====================================================

        var assetWarehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.IsAssetWarehouse);

        if (assetWarehouse == null)
        {
            ModelState.AddModelError(
                "",
                "انبار تجهیزات در سیستم پیدا نشد.");

            return await ReturnPageAsync(pcId);
        }


        // =====================================================
        // 7. بررسی موجودی کالا
        // =====================================================

        var stock = await _context.WarehouseStocks
            .FirstOrDefaultAsync(s =>
                s.WarehouseId == assetWarehouse.Id &&
                s.ProductId == component.ProductId);

        if (stock == null || stock.Quantity < 1)
        {
            ModelState.AddModelError(
                "",
                $"موجودی کالای «{component.Product?.ProductName}» در انبار تجهیزات کافی نیست.");

            return await ReturnPageAsync(pcId);
        }


        // =====================================================
        // 8. شروع Transaction
        // =====================================================

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // =================================================
            // 9. ایجاد AssemblyComponent
            // =================================================

            var assemblyComponent = new AssemblyComponent
            {
                FromAssetId = component.Id,

                PcAssetId = pc.Id,

                ComponentAssetId = component.Id,

                InstalledAt = DateTime.Now,

                InstalledBy = User.Identity?.Name,

                Notes =
                    $"افزودن قطعه به اسمبل #{pc.AssemblyNumber}"
            };

            _context.AssemblyComponents.Add(assemblyComponent);


            // =================================================
            // 10. تغییر وضعیت قطعه
            // =================================================

            component.Status = AssetStatus.Installed;

            component.WarehouseId = assetWarehouse.Id;


            // =================================================
            // 11. کاهش موجودی
            // =================================================

            stock.Quantity -= 1;

            stock.UpdatedAt = DateTime.Now;


            // =================================================
            // 12. ایجاد حواله خروج
            // =================================================

            var lastIssueNumber =
                await _context.WarehouseIssues
                    .MaxAsync(x => (int?)x.IssueNumber) ?? 0;

            var issue = new WarehouseIssue
            {
                IssueNumber = lastIssueNumber + 1,

                IssueDate = DateTime.Now,

                CreatedAt = DateTime.Now,

                CreatedBy = User.Identity?.Name,

                WarehouseId = assetWarehouse.Id,

                Status = DocumentStatus.Posted,

                Description =
                    $"خروج قطعه جهت افزودن به اسمبل #{pc.AssemblyNumber}"
            };

            _context.WarehouseIssues.Add(issue);

            await _context.SaveChangesAsync();


            // =================================================
            // 13. ایجاد آیتم حواله
            // =================================================

            var issueItem = new WarehouseIssueItem
            {
                IssueId = issue.Id,

                RowNumber = 1,

                ProductId = (int)component.ProductId,

                Quantity = 1,

                Description =
                    $"قطعه {component.Name} جهت اسمبل #{pc.AssemblyNumber}"
            };

            _context.WarehouseIssueItems.Add(issueItem);

            await _context.SaveChangesAsync();


            // =================================================
            // 14. ثبت تراکنش انبار
            // =================================================

            _context.InventoryTransactions.Add(
                new InventoryTransaction
                {
                    WarehouseId = assetWarehouse.Id,

                    ProductId = (int)component.ProductId,

                    Quantity = -1,

                    Type = InventoryTransactionType.Issue,

                    TransactionDate = DateTime.Now,

                    IssueItemId = issueItem.Id,

                    Description =
                        $"خروج قطعه {component.Name} از انبار جهت اسمبل #{pc.AssemblyNumber}",

                    CreatedAt = DateTime.Now,

                    CreatedBy =
                        User.Identity?.Name ?? "System"
                });


            // =================================================
            // 15. ذخیره نهایی
            // =================================================

            await _context.SaveChangesAsync();


            // =================================================
            // 16. Commit
            // =================================================

            await transaction.CommitAsync();


            TempData["Success"] =
                $"قطعه «{component.Name}» با موفقیت به سیستم اضافه شد.";

            return RedirectToPage(
                "/InternalComponents/Details",
                new { id = pc.Id });
        }
        catch
        {
            await transaction.RollbackAsync();

            TempData["Error"] =
                "در هنگام افزودن قطعه خطایی رخ داد. هیچ تغییری اعمال نشد.";

            return RedirectToPage(
                "/InternalComponents/Details",
                new { id = pc.Id });
        }
    }


    // =========================================================
    // بازگرداندن فرم همراه با خطا
    // =========================================================

    private async Task<IActionResult> ReturnPageAsync(int pcId)
    {
        var pc = await _context.Assets
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a =>
                a.Id == pcId &&
                a.IsAssembled &&
                a.Category != null &&
                a.Category.HasInternalComponent);

        if (pc == null)
            return NotFound();

        PcAsset = pc;

        InstalledCategories = await _context.Categories
            .Where(c => c.Type == AssetCategoryType.Installed)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Page();
    }
}