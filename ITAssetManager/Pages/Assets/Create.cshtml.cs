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
        // بررسی موجودی
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

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // =====================================================
            // 1. ایجاد تجهیز
            // =====================================================

            Asset.CreatedAt = DateTime.Now;
            Asset.PurchaseDate = TextPurchaseDate.ToMiladi();
            Asset.WarrantyExpiry = TextWarrantyExpiryDate.ToMiladi();
            Asset.ProductId = (int)ProductId;
            Asset.WarehouseId= 4;
            Asset.EmployeeName = "افشین کریمی";
            Asset.EmployeeId = 1147;
            _context.Assets.Add(Asset);

            await _context.SaveChangesAsync();


            // =====================================================
            // 2. ثبت تخصیص اولیه تجهیز
            // =====================================================

            if (Asset.EmployeeId.HasValue ||
                Asset.DepartmentId.HasValue)
            {
                var assignment = new AssetAssignment
                {
                    AssetId = Asset.Id,

                    ToEmployeeId = Asset.EmployeeId,
                    ToDepartmentId = Asset.DepartmentId,
                    ToLocation = Asset.Location,

                    AssignedAt = DateTime.Now,

                    Reason = "ثبت اولیه تجهیز",

                    AssignedBy = User.Identity?.Name
                };

                _context.AssetAssignments.Add(assignment);
            }


            // =====================================================
            // 3. ثبت مشخصات فنی تجهیز
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


            // =====================================================
            // 4. تولید شماره حواله
            // =====================================================

            var maxIssueNumber =
                await _context.WarehouseIssues
                    .Select(x => (int?)x.IssueNumber)
                    .MaxAsync() ?? 0;

            var issueNumber = maxIssueNumber + 1;


            // =====================================================
            // 5. ایجاد حواله خروج
            // =====================================================

            var issue = new WarehouseIssue
            {
                IssueNumber = issueNumber,

                IssueDate = DateTime.Now,

                WarehouseId = WarehouseId.Value,

                EmployeeId = Asset.EmployeeId,

                EmployeeName = Asset.EmployeeName,

                CreatedBy = User.Identity?.Name ?? "سیستم",

                CreatedAt = DateTime.Now,

                Description =
                    $"خروج کالا جهت ثبت تجهیز - تجهیز شماره {Asset.Id}",

                Status = DocumentStatus.Posted,

                Source = IssueSource.AssetCreation
            };

            _context.WarehouseIssues.Add(issue);

            await _context.SaveChangesAsync();


            // =====================================================
            // 6. ایجاد آیتم حواله
            // =====================================================

            var issueItem = new WarehouseIssueItem
            {
                IssueId = issue.Id,

                RowNumber = 1,

                ProductId = ProductId.Value,

                Quantity = 1
            };

            _context.WarehouseIssueItems.Add(issueItem);

            await _context.SaveChangesAsync();


            // =====================================================
            // 7. کاهش موجودی انبار
            // =====================================================

            stock.Quantity -= 1;
            stock.UpdatedAt = DateTime.Now;


            // =====================================================
            // 8. ثبت تراکنش انبار
            // =====================================================

            var inventoryTransaction = new InventoryTransaction
            {
                WarehouseId = WarehouseId.Value,

                ProductId = ProductId.Value,

                Quantity = 1,

                Type = InventoryTransactionType.Issue,

                TransactionDate = issue.IssueDate,

                IssueItemId = issueItem.Id,

                Description =
                    $"حواله شماره {issue.IssueNumber} بابت ثبت تجهیز شماره {Asset.Id}",

                CreatedAt = DateTime.Now,

                CreatedBy = User.Identity?.Name ?? "سیستم"
            };

            _context.InventoryTransactions.Add(inventoryTransaction);


            // =====================================================
            // 9. ذخیره نهایی
            // =====================================================

            await _context.SaveChangesAsync();


            // =====================================================
            // 10. تأیید Transaction
            // =====================================================

            await transaction.CommitAsync();


            TempData["Success"] =
                $"تجهیز با موفقیت ثبت شد. حواله شماره {issue.IssueNumber} نیز ایجاد گردید.";

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
            await _context.Warehouses
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
