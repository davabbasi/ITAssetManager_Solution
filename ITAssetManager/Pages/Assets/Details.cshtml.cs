using System.Security.Claims;
using System.Security.Cryptography.Xml;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Assets;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public DetailsModel(ApplicationDbContext context) => _context = context;
    public AssemblyComponent? InstalledIn { get; set; }
    public Asset Asset { get; set; } = null!;
    public List<AssetAssignment> Assignments { get; set; } = new();
    [BindProperty] public AssetAssignment Assignment { get; set; } = new();
    public WarehouseTransfer Transfer { get; set; } = new();
    public WarehouseTransferItem TransferItem { get; set; } = new();
    public List<MaintenanceLog> MaintenanceLogs { get; set; } = new();
    public List<AssemblyComponent> InstalledList { get; set; }
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var asset = await _context.Assets
            .Include(a => a.Category)
            .Include(a => a.SpecValues)
            .ThenInclude(sv => sv.Specification)
            .Include(a => a.SpecValues)
            .ThenInclude(sv => sv.SpecValue)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null) return NotFound();
        Asset = asset;




        InstalledIn = await _context.AssemblyComponents
            .Include(x => x.PcAsset).OrderByDescending(x => x.InstalledAt)
            .FirstOrDefaultAsync(x =>
            x.ComponentAssetId == Asset.Id &&
            x.RemovedAt == null);

        InstalledList = await _context.AssemblyComponents
            .Include(x => x.PcAsset)
            .Where(x => x.ComponentAssetId == Asset.Id)
           .ToListAsync();


        Assignments = await _context.AssetAssignments
            .Where(a => a.AssetId == id)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

        MaintenanceLogs = await _context.MaintenanceLogs
            .Where(m => m.AssetId == id)
            .OrderByDescending(m => m.StartDate)
            .ToListAsync();


        return Page();
    }

    public async Task<IActionResult> OnPostScrapAsync(int id)
    {
        // ================================
        // 1. پیدا کردن تجهیز
        // ================================
        var asset = await _context.Assets
            .Include(a => a.Warehouse)
            .ThenInclude(w => w.Keeper)
            .Include(a => a.Product)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null)
            return NotFound();

        // ================================
        // 2. بررسی وضعیت تجهیز
        // ================================

        if (asset.Status != AssetStatus.Active)
        {
            TempData["AssetScrapError"] =
                "فقط تجهیزات فعال قابلیت اسقاط شدن را دارند.";

            return RedirectToPage(new { id });
        }


        // ============================================================
        // 3. شروع Transaction
        //    اگر هر مرحله خطا داشت، همه چیز Rollback می‌شود
        // ============================================================

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // ================================
            // 4. پیدا کردن انبار اسقاط
            // ================================

            var scrapWarehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Type == WarehouseType.Scrap);

            if (scrapWarehouse == null)
            {
                TempData["AssetScrapError"] =
                    "انبار اسقاط در سیستم تعریف نشده است.";
                await transaction.RollbackAsync();
                return RedirectToPage(new { id });
            }

            // ================================
            // 5. پیدا کردن انبار فعلی
            // ================================

            var sourceWarehouse = await _context.Warehouses
                .Include(w => w.Keeper)
                .FirstOrDefaultAsync(w => w.Id == asset.WarehouseId);
            if (sourceWarehouse == null)
            {
                TempData["AssetScrapError"] =
                    "انبار فعلی تجهیز پیدا نشد.";
                await transaction.RollbackAsync();
                return RedirectToPage(new { id });
            }


            // ================================
            // 6.  پیدا کردن انباردار انبار فعلی
            // ================================
            var keeper = sourceWarehouse.Keeper;

            if (keeper == null ||
                string.IsNullOrWhiteSpace(keeper.PersonnelNumber))
            {
                throw new Exception(
                    "انباردار انبار فعلی مشخص نشده است.");
            }


            // ================================
            // 7. پیدا کردن کارمند متناظر با انباردار
            // ================================
            var keeperEmployee = await _context.VwEmployees
           .FirstOrDefaultAsync(e =>
               e.EmployeeCode == keeper.PersonnelNumber);

            if (keeperEmployee == null)
            {
                throw new Exception(
                    "کارمند متناظر با انباردار پیدا نشد.");
            }

            // ================================
            // 8. اگر تجهیز دست شخص/واحدی است باید جابجایی ثبت شود
            // ================================
            if (asset.EmployeeId.HasValue ||asset.DepartmentId.HasValue)
            {
                var assignment = new AssetAssignment
                {
                    AssetId = asset.Id,

                    // از کجا
                    FromEmployeeId = asset.EmployeeId,
                    FromEmployeeName = asset.EmployeeName,

                    FromDepartmentId = asset.DepartmentId,
                    FromDepartmentName = asset.DepartmentName,

                    FromLocation = asset.Location,

                    // به کجا
                    ToEmployeeId = keeperEmployee.Id,
                    ToEmployeeName = keeperEmployee.FullName,

                    ToDepartmentId = keeperEmployee.DepartmentId,
                    ToDepartmentName = keeperEmployee.DepartmentName,

                    ToLocation = sourceWarehouse.WarehouseName,

                    AssignedAt = DateTime.Now,
                    AssignedBy = User.Identity?.Name,

                    Reason = "تحویل جهت اسقاط"
                };

                _context.AssetAssignments.Add(assignment);
            }



            // ================================
            // بررسی موجودی کالا در انبار فعلی.9
            // ================================

            var stock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(x =>
                x.WarehouseId == sourceWarehouse.Id &&
                x.ProductId == asset.ProductId);
            if (stock == null || stock.Quantity < 1)
            {
                TempData["AssetScrapError"] =
                    "موجودی کالای مربوط به این تجهیز در انبار کافی نیست.";

                await transaction.RollbackAsync();
                return RedirectToPage(new { id });
            }

            // ================================
            // 10.ایجاد انتقال
            // ================================
            
            var transfer = new WarehouseTransfer
            {
                SourceWarehouseId = sourceWarehouse.Id,
                DestinationWarehouseId = scrapWarehouse.Id,
                TransferDate = DateTime.Now,
                TransferNumber =(await _context.WarehouseTransfers
                    .Select(x => (int?)x.TransferNumber)
                    .MaxAsync() ?? 0) + 1,
                Status = DocumentStatus.Posted,
                Description =
                $"انتقال جهت اسقاط تجهیز «{asset.Name}»",
                CreatedBy =User.Identity?.Name ?? "سیستم",
                CreatedAt = DateTime.Now
            };
            _context.WarehouseTransfers.Add(transfer);

            // ================================
            //  11.ایجاد قلم انتقال
            // ================================
            var transferItem = new WarehouseTransferItem
            {
                WarehouseTransfer = transfer,
                RowNumber = 1,
                ProductId = asset.ProductId,
                Quantity = 1,
                Description =$"اسقاط تجهیز «{asset.Name}»"
            };
            _context.WarehouseTransferItems.Add(transferItem);

            // ================================
            // 12.کاهش موجودی انبار فعلی
            // ================================

            stock.Quantity -= 1;
            stock.UpdatedAt = DateTime.Now;

            // ================================
            // 13.افزایش موجودی انبار اسقاط
            // ================================

            var scrapStock = await _context.WarehouseStocks
            .FirstOrDefaultAsync(x =>
                x.WarehouseId == scrapWarehouse.Id &&
                x.ProductId == asset.ProductId);

            if (scrapStock == null)
            {
                scrapStock = new WarehouseStock
                {
                    WarehouseId = scrapWarehouse.Id,
                    ProductId = asset.ProductId,
                    Quantity = 1,
                    UpdatedAt = DateTime.Now
                };

                _context.WarehouseStocks.Add(scrapStock);
            }
            else
            {
                scrapStock.Quantity += 1;
                scrapStock.UpdatedAt = DateTime.Now;
            }


            // ================================
            // 14.ذخیره انتقال
            // ================================      

            await _context.SaveChangesAsync();

            // ================================
            // 15.ثبت تراکنش خروج از انبار فعلی
            // ================================          
            var transferOutTransaction = new InventoryTransaction
            {
                WarehouseId = sourceWarehouse.Id,
                ProductId = asset.ProductId,
                Quantity = 1,
                Type = InventoryTransactionType.TransferOut,
                TransactionDate = DateTime.Now,
                TransferItemId = transferItem.Id,
                Description =$"انتقال جهت اسقاط تجهیز «{asset.Name}»",
                CreatedAt = DateTime.Now,
                CreatedBy =User.Identity?.Name ?? "سیستم"
            };
            _context.InventoryTransactions.Add(transferOutTransaction);

            // ================================
            //  16.ثبت تراکنش ورود به انبار اسقاط
            // ================================
            var transferInTransaction = new InventoryTransaction
            {
                WarehouseId = scrapWarehouse.Id,
                ProductId = asset.ProductId,
                Quantity = 1,
                Type = InventoryTransactionType.TransferIn,
                TransactionDate = DateTime.Now,
                TransferItemId = transferItem.Id,
                Description =$"ورود به انبار اسقاط تجهیز «{asset.Name}»",
                CreatedAt = DateTime.Now,
                CreatedBy =User.Identity?.Name ?? "سیستم"
            };
            _context.InventoryTransactions.Add(transferInTransaction);
            await _context.SaveChangesAsync();

            // ================================
            //  17.تغییر وضعیت
            // ================================          
            asset.Status = AssetStatus.Scrapped;
            asset.StatusNote = "تجهیز اسقاط شده است.";

            // ================================
            //  18.تغییر انبار تجهی
            // ================================         
            asset.WarehouseId = scrapWarehouse.Id;

            // ================================
            // 19.تجهیز دیگر تحویل شخص نیست
            // ================================

            asset.EmployeeId = null;
            asset.EmployeeName = null;
            asset.DepartmentId = null;
            asset.DepartmentName = null;
            asset.Location = scrapWarehouse.WarehouseName;
            asset.UpdatedAt = DateTime.Now;

            // ================================
            // 20. ذخیره نهایی
            // ================================
            await _context.SaveChangesAsync();

            // ================================
            // 21. نهایی کردن Transaction
            // ================================
            await transaction.CommitAsync();

            // ================================
            // 22. پیام موفقیت
            // ================================
            TempData["AssetScrapSuccess"] =
                "تجهیز با موفقیت اسقاط شد.";
            return RedirectToPage(new { id });
        }

        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["AssetScrapError"] =
                $"در هنگام اسقاط تجهیز خطایی رخ داد: {ex.Message}";
            return RedirectToPage(new { id });
        }
    }
}
