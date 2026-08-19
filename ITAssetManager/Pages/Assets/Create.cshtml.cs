using ITAssetManager.Convertor;
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
    [BindProperty] public string TextPurchaseDate { get; set; }
    [BindProperty] public string TextWarrantyExpiryDate { get; set; }
    public SelectList CategoryList { get; set; } = null!;
    public SelectList DepartmentList { get; set; } = null!;
    public SelectList EmployeeList { get; set; } = null!;
    public List<CategorySpecification> CategorySpecifications { get; set; } = new();
    public SelectList VendorList { get; set; } = null!;
    [BindProperty] public int? WarehouseId { get; set; }
    [BindProperty] public int? ProductId { get; set; }
    public SelectList WarehouseList { get; set; } = null!;
    [BindProperty]
    public Dictionary<string, string> SpecValues
    {
        get;
        set;
    }
        = new();

    public async Task OnGetAsync()
    {

        await LoadSelectListsAsync();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        // -----------------------------
        // اعتبارسنجی انبار و کالا
        // -----------------------------

        if (!WarehouseId.HasValue)
        {
            ModelState.AddModelError(
                nameof(WarehouseId),
                "لطفاً انبار را انتخاب کنید.");

            await LoadSelectListsAsync();
            return Page();
        }

        if (!ProductId.HasValue)
        {
            ModelState.AddModelError(
                nameof(ProductId),
                "لطفاً کالا را انتخاب کنید.");

            await LoadSelectListsAsync();
            return Page();
        }

        // -----------------------------
        // بررسی موجودی انبار کالای انتخاب شده
        // -----------------------------

        var stock = await _context.WarehouseStocks
            .FirstOrDefaultAsync(x =>
                x.WarehouseId == WarehouseId.Value &&
                x.ProductId == ProductId.Value);

        if (stock == null || stock.Quantity < 1)
        {
            ModelState.AddModelError(
                nameof(ProductId),
                "موجودی این کالا در انبار انتخاب شده کافی نیست.");

            await LoadSelectListsAsync();
            return Page();
        }

         

        // -----------------------------
        // شروع Transaction
        // -----------------------------

        await using var transaction =await _context.Database.BeginTransactionAsync();
        try
        {

          
            // =====================================================
            // 1. پیدا کردن انبار تجهیزات
            // =====================================================
            var assetWarehouse = await _context.Warehouses
                .Include(w=>w.Keeper)             
               .FirstOrDefaultAsync(w => w.Type == WarehouseType.Asset);

            if (assetWarehouse == null)
            {
                TempData["AssetWarehouseError"] =
                    "انبار تجهیزات در سیستم تعریف نشده است.";
                await transaction.RollbackAsync();
                await LoadSelectListsAsync();
                return Page();
            }

            // =====================================================
            // 2.پیدا کردن کارمند متناظر با انباردار
            // =====================================================

            var keeperEmployee = await _context.VwEmployees
                .FirstOrDefaultAsync(e =>
                e.EmployeeCode == assetWarehouse.Keeper.PersonnelNumber);

            if (keeperEmployee == null)
            {
                throw new Exception(
                    "کارمند متناظر با انباردار پیدا نشد.");
            }

            // =====================================================
            // 3. ایجاد تجهیز
            // =====================================================

            Asset.CreatedAt = DateTime.Now;
            Asset.PurchaseDate = TextPurchaseDate.ToMiladi();
            Asset.WarrantyExpiry = TextWarrantyExpiryDate.ToMiladi();
            Asset.ProductId = (int)ProductId;
            Asset.WarehouseId= assetWarehouse.Id;
            Asset.EmployeeName = assetWarehouse.Keeper.FullName;
            Asset.EmployeeId = keeperEmployee.Id;
            _context.Assets.Add(Asset);
            await _context.SaveChangesAsync();


            // =====================================================
            // 4. ثبت تخصیص اولیه تجهیز
            // =====================================================

            if (Asset.EmployeeId.HasValue ||Asset.DepartmentId.HasValue)
            {
                var assignment = new AssetAssignment
                {
                    AssetId = Asset.Id,
                    ToEmployeeId = Asset.EmployeeId,
                    ToEmployeeName = Asset.EmployeeName,
                    ToDepartmentId = Asset.DepartmentId,
                    ToLocation = Asset.Location,
                    AssignedAt = DateTime.Now,
                    Reason = "ثبت اولیه تجهیز",
                    AssignedBy = User.Identity?.Name
                };

                _context.AssetAssignments.Add(assignment);
            }


            // =====================================================
            // 5. ثبت مشخصات فنی تجهیز
            // =====================================================

            if (SpecValues.Any())
            {
                foreach (var (specDefId, specValId) in SpecValues)
                {
                    if (!string.IsNullOrEmpty(specValId) &&
                        specValId != "انتخاب کنید..." &&
                        int.TryParse(specDefId, out int definitionId) &&
                        int.TryParse(specValId, out int valueId))
                    {
                        _context.AssetSpecValues.Add(
                            new AssetSpecificationValue
                            {
                                AssetId = Asset.Id,
                                SpecDefinitionId = definitionId,
                                SpecValueId = valueId
                            });
                    }
                }
            }


            // ================================
            // 6.ایجاد انتقال
            // ================================

            var transfer = new WarehouseTransfer
            {
                SourceWarehouseId = (int)WarehouseId,
                DestinationWarehouseId = assetWarehouse.Id,
                TransferDate = DateTime.Now,
                TransferNumber = (await _context.WarehouseTransfers
                    .Select(x => (int?)x.TransferNumber)
                    .MaxAsync() ?? 0) + 1,
                Status = DocumentStatus.Posted,
                Description =
                $"انتقال جهت ایجاد اولیه تجهیز «{Asset.Name}»",
                CreatedBy = User.Identity?.Name ?? "سیستم",
                CreatedAt = DateTime.Now
            };
            _context.WarehouseTransfers.Add(transfer);

            // ================================
            //  7.ایجاد قلم انتقال
            // ================================
            var transferItem = new WarehouseTransferItem
            {
                WarehouseTransfer = transfer,
                RowNumber = 1,
                ProductId = (int)Asset.ProductId,
                Quantity = 1,
                Description = $"ایجاد تجهیز «{Asset.Name}»"
            };
            _context.WarehouseTransferItems.Add(transferItem);

            // ================================
            // 8.کاهش موجودی انبار فعلی
            // ================================

            stock.Quantity -= 1;
            stock.UpdatedAt = DateTime.Now;

            // ================================
            // 9.افزایش موجودی انبار تجهیزات
            // ================================

            var assetStock = await _context.WarehouseStocks
            .FirstOrDefaultAsync(x =>
                x.WarehouseId == assetWarehouse.Id &&
                x.ProductId == Asset.ProductId);

            if (assetStock == null)
            {
                assetStock = new WarehouseStock
                {
                    WarehouseId = assetWarehouse.Id,
                    ProductId = (int)Asset.ProductId,
                    Quantity = 1,
                    UpdatedAt = DateTime.Now
                };

                _context.WarehouseStocks.Add(assetStock);
            }
            else
            {
                assetStock.Quantity += 1;
                assetStock.UpdatedAt = DateTime.Now;
            }


            // ================================
            // 10.ذخیره انتقال
            // ================================      

            await _context.SaveChangesAsync();

            // ================================
            // 11.ثبت تراکنش خروج از انبار کالا
            // ================================          
            var transferOutTransaction = new InventoryTransaction
            {
                WarehouseId = (int)WarehouseId,
                ProductId = (int)ProductId,
                Quantity = -1,
                Type = InventoryTransactionType.TransferOut,
                TransactionDate = DateTime.Now,
                TransferItemId = transferItem.Id,
                Description = $"انتقال جهت ایجاد اولیه تجهیز «{Asset.Name}»",
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "سیستم"
            };
            _context.InventoryTransactions.Add(transferOutTransaction);

            // ================================
            //  12.ثبت تراکنش ورود به انبار تجهیزات
            // ================================
            var transferInTransaction = new InventoryTransaction
            {
                WarehouseId = assetWarehouse.Id,
                ProductId = (int)ProductId,
                Quantity = 1,
                Type = InventoryTransactionType.TransferIn,
                TransactionDate = DateTime.Now,
                TransferItemId = transferItem.Id,
                Description = $"ورود به انبار تجهیزات تجهیز «{Asset.Name}»",
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "سیستم"
            };
            _context.InventoryTransactions.Add(transferInTransaction);
            await _context.SaveChangesAsync();

            // =====================================================
            // 13. تأیید Transaction
            // =====================================================

            await transaction.CommitAsync();
            TempData["AssetCreateSuccess"] =
                $"تجهیز با موفقیت ثبت شد. انتقال شماره {transfer.TransferNumber} نیز ایجاد گردید.";
            return RedirectToPage(
                "/Assets/Details",
                new { id = Asset.Id });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                "",
                "در هنگام ثبت تجهیز و خروج کالا از انبار خطایی رخ داد. هیچ اطلاعاتی ثبت نشد.");

            await LoadSelectListsAsync();

            return Page();
        }
    }
    private async Task LoadSelectListsAsync()
    {
        WarehouseList = new SelectList(
            await _context.Warehouses.Where(w=>w.IsITWarehouse==true&&w.Type==WarehouseType.Main)
                .OrderBy(w => w.WarehouseName)
                .ToListAsync(),
            "Id",
            "WarehouseName");

        CategoryList = new SelectList(
            await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync(),
            "Id",
            "Name");

        DepartmentList = new SelectList(
            await _context.VwDepartments
                .OrderBy(d => d.Name)
                .ToListAsync(),
            "Id",
            "Name");

        EmployeeList = new SelectList(
            await _context.VwEmployees
                .OrderBy(e => e.FullName)
                .Select(e => new
                {
                    e.Id,
                    Name = e.FullName + " - " + e.DepartmentName
                })
                .ToListAsync(),
            "Id",
            "Name");

        VendorList = new SelectList(
            await _context.Vendors
                .Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .ToListAsync(),
            "Id",
            "Name");
    }
    private async Task<List<Specification>> GetSpecificationsAsync(int categoryId)
    {

        return await _context.Specifications
            .Include(s => s.SpecValues)
            .Where(s => s.CategorySpecifications
            .Any(cs => cs.CategoryId == categoryId))
            .OrderBy(s => s.SortOrder)
            .ToListAsync();
    }
    public async Task<IActionResult> OnGetSpecsAsync(int categoryId)
    {
        var specs = await GetSpecificationsAsync(categoryId);
        return Partial("_SpecsPartial", specs);
    }
    public async Task<IActionResult> OnGetWarehouseProductsAsync(int warehouseId)
    {
        var products = await _context.WarehouseStocks
            .Where(x =>
                x.WarehouseId == warehouseId &&
                x.Quantity > 0)
            .Include(x => x.Product)
            .OrderBy(x => x.Product.ProductName)
            .Select(x => new
            {
                id = x.ProductId,
                name = x.Product.ProductName,
                quantity = x.Quantity
            })
            .ToListAsync();

        return new JsonResult(products);
    }
    public async Task<IActionResult> OnGetProductNameAsync(int productId)
    {
        var productName = await _context.Products
            .Where(p => p.Id == productId)
            .Select(p => p.ProductName)
            .FirstOrDefaultAsync();

        if (productName == null)
            return NotFound();

        return Content(productName, "text/plain; charset=utf-8");
    }

}
