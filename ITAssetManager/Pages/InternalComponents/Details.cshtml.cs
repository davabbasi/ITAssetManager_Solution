using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assembly;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public DetailsModel(ApplicationDbContext context) => _context = context;

    public Asset PcAsset { get; set; } = null!;
    public List<AssemblyComponent> ActiveComponents { get; set; } = new();
    public List<AssemblyComponent> RemovedComponents { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var pc = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id );
        if (pc == null) return NotFound();
        PcAsset = pc;

        var components = await _context.AssemblyComponents
            .Include(c => c.ComponentAsset)
                .ThenInclude(a => a!.Category)
            .Where(c => c.PcAssetId == id)
            .OrderByDescending(c => c.InstalledAt)
            .ToListAsync();

        ActiveComponents = components.Where(c => c.RemovedAt == null).ToList();
        RemovedComponents = components.Where(c => c.RemovedAt != null).ToList();

        return Page();
    }
    public async Task<IActionResult> OnPostRemoveComponentAsync(int id,int assemblyComponentId)
    {
        // =========================================================
        // 1. پیدا کردن سیستم اسمبل‌شده
        // =========================================================

        var pc = await _context.Assets
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                a.IsAssembled);

        if (pc == null)
        {
            TempData["AssemblyError"] =
                "سیستم اسمبل‌شده پیدا نشد.";

            return RedirectToPage(new { id });
        }


        // =========================================================
        // 2. پیدا کردن AssemblyComponent فعال
        // =========================================================

        var assemblyComponent =
            await _context.AssemblyComponents
                .Include(x => x.ComponentAsset)
                    .ThenInclude(a => a!.Product)
                .FirstOrDefaultAsync(x =>
                    x.Id == assemblyComponentId &&
                    x.PcAssetId == id &&
                    x.RemovedAt == null);

        if (assemblyComponent == null)
        {
            TempData["AssemblyError"] =
                "قطعه فعال موردنظر در این اسمبل پیدا نشد.";

            return RedirectToPage(new { id });
        }


        // =========================================================
        // 3. پیدا کردن قطعه
        // =========================================================

        var component = assemblyComponent.ComponentAsset;

        if (component == null)
        {
            TempData["AssemblyError"] =
                "اطلاعات قطعه پیدا نشد.";

            return RedirectToPage(new { id });
        }


        // =========================================================
        // 4. پیدا کردن انبار تجهیزات
        // =========================================================

        var assetWarehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.IsAssetWarehouse);

        if (assetWarehouse == null)
        {
            TempData["AssemblyError"] =
                "انبار تجهیزات در سیستم پیدا نشد.";

            return RedirectToPage(new { id });
        }


        // =========================================================
        // 5. شروع Transaction
        // =========================================================

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // =====================================================
            // 6. پیدا کردن موجودی کالا
            // =====================================================

            var stock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(s =>
                    s.WarehouseId == assetWarehouse.Id &&
                    s.ProductId == component.ProductId);

            if (stock == null)
            {
                stock = new WarehouseStock
                {
                    WarehouseId = assetWarehouse.Id,

                    ProductId = (int)component.ProductId,

                    Quantity = 0,

                    UpdatedAt = DateTime.Now
                };

                _context.WarehouseStocks.Add(stock);
            }


            // =====================================================
            // 7. افزایش موجودی
            // =====================================================

            stock.Quantity += 1;
            stock.UpdatedAt = DateTime.Now;


            // =====================================================
            // 8. ثبت خروج قطعه از اسمبل
            // =====================================================

            assemblyComponent.RemovedAt = DateTime.Now;

            assemblyComponent.RemovedBy =User.Identity?.Name;

            assemblyComponent.Notes = $"خروج از اسمبل #{pc.AssemblyNumber} و بازگشت به انبار تجهیزات";


            // =====================================================
            // 9. تغییر وضعیت قطعه
            // =====================================================

            component.Status = AssetStatus.InStorage;

            component.WarehouseId = assetWarehouse.Id;


            // =====================================================
            // 10. ایجاد رسید برگشت
            // =====================================================

            var lastReceiptNumber =
                await _context.WarehouseReceipts
                    .MaxAsync(x => (int?)x.ReceiptNumber) ?? 0;

            var receipt = new WarehouseReceipt
            {
                ReceiptNumber = lastReceiptNumber + 1,

                ReceiptDate = DateTime.Now,

                CreatedAt = DateTime.Now,

                CreatedBy = User.Identity?.Name,

                WarehouseId = assetWarehouse.Id,

                Description =
                    $"برگشت قطعه از اسمبل #{pc.AssemblyNumber}"
            };

            _context.WarehouseReceipts.Add(receipt);

            await _context.SaveChangesAsync();


            // =====================================================
            // 11. ایجاد ردیف رسید
            // =====================================================

            var receiptItem = new WarehouseReceiptItem
            {
                ReceiptId = receipt.Id,

                RowNumber = 1,

                ProductId = (int)component.ProductId,

                Quantity = 1,

                Description =
                    $"برگشت قطعه {component.Name} از اسمبل #{pc.AssemblyNumber}"
            };

            _context.WarehouseReceiptItems.Add(receiptItem);

            await _context.SaveChangesAsync();


            // =====================================================
            // 12. ثبت تراکنش رسید
            // =====================================================

            _context.InventoryTransactions.Add(
                new InventoryTransaction
                {
                    WarehouseId = assetWarehouse.Id,

                    ProductId = (int)component.ProductId,

                    Quantity = 1,

                    Type = InventoryTransactionType.Receipt,

                    TransactionDate = DateTime.Now,

                    ReceiptItemId = receiptItem.Id,

                    Description =
                        $"برگشت قطعه {component.Name} از اسمبل #{pc.AssemblyNumber}",

                    CreatedAt = DateTime.Now,

                    CreatedBy = User.Identity?.Name ?? "System"
                });


            // =====================================================
            // 13. ذخیره
            // =====================================================

            await _context.SaveChangesAsync();


            // =====================================================
            // 14. Commit
            // =====================================================

            await transaction.CommitAsync();


            TempData["AssemblySuccess"] =
                $"قطعه «{component.Name}» با موفقیت از سیستم خارج و به انبار تجهیزات برگشت داده شد.";

            return RedirectToPage(new { id });
        }
        catch
        {
            await transaction.RollbackAsync();

            TempData["AssemblyError"] =
                "در هنگام خروج قطعه خطایی رخ داد. هیچ تغییری اعمال نشد.";

            return RedirectToPage(new { id });
        }
    }
}