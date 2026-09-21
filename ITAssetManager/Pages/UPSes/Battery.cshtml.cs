using System.Net.NetworkInformation;
using System.Security.Claims;
using ITAssetManager.Convertor;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static ITAssetManager.Pages.Printers.CartridgeModel;

namespace ITAssetManager.Pages.UPSes
{
    [Authorize]

    public class BatteryModel : PageModel
    {

        private readonly ApplicationDbContext _context;

        public BatteryModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Asset UPS { get; set; } = null!;

        public List<BatteryConsumption> Consumptions { get; set; } = new();

        // -----------------------------
        // اطلاعات فرم مصرف باتری
        // -----------------------------

        [BindProperty]
        public int ProductId { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        [BindProperty]
        public string ConsumptionShamsiDate { get; set; } = string.Empty;

        [BindProperty]
        public string? Description { get; set; }

        public class BatteryProductViewModel
        {
            public int ProductId { get; set; }

            public string ProductName { get; set; } = string.Empty;

            public decimal Quantity { get; set; }
        }

        public List<BatteryProductViewModel> BatteryProduct { get; set; } = new();
        public async Task<IActionResult> OnGetAsync(int id)
        {
            await LoadUPS(id);

            if (UPS == null)
                return NotFound();

            await LoadBatteryProducts();
            await LoadConsumptions(id);

            ConsumptionShamsiDate = DateTime.Now.ToShamsi();

            return Page();
        }

        // ==========================================
        // ثبت مصرف باتری
        // ==========================================

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await LoadUPS(id);

            if (UPS == null)
                return NotFound();

            await LoadBatteryProducts();

            // -----------------------------
            // اعتبارسنجی اولیه
            // -----------------------------

            if (ProductId <= 0)
            {
                ModelState.AddModelError(
                    nameof(ProductId),
                    "لطفاً UPS را انتخاب کنید.");
            }

            if (Quantity <= 0)
            {
                ModelState.AddModelError(
                    nameof(Quantity),
                    "تعداد باید بیشتر از صفر باشد.");
            }

            var consumptionDate = ConsumptionShamsiDate.ToMiladi();

            if (!consumptionDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(ConsumptionShamsiDate),
                    "تاریخ مصرف را به‌درستی وارد کنید.");
            }

            if (!ModelState.IsValid)
            {
                await LoadConsumptions(id);
                return Page();
            }

            // -----------------------------
            // پیدا کردن انبار اصلی IT
            // -----------------------------

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w =>
                    w.IsITWarehouse &&
                    w.Type == WarehouseType.Main);

            if (warehouse == null)
            {
                ModelState.AddModelError(
                    "",
                    "انبار اصلی انفورماتیک پیدا نشد.");

                await LoadConsumptions(id);
                return Page();
            }

            // -----------------------------
            // بررسی موجودی
            // -----------------------------

            var stock = await _context.WarehouseStocks
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == warehouse.Id &&
                    x.ProductId == ProductId);

            if (stock == null || stock.Quantity < Quantity)
            {
                ModelState.AddModelError(
                    "",
                    $"موجودی باتری «{stock?.Product?.ProductName ?? "انتخاب شده"}» کافی نیست.");

                await LoadConsumptions(id);
                return Page();
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var userName =
                    User.FindFirstValue(ClaimTypes.Name)
                    ?? User.Identity?.Name
                    ?? "سیستم";

                // ==========================================
                // 1. ساخت حواله انبار
                // ==========================================

                var maxIssueNumber =
                    await _context.WarehouseIssues
                        .Select(x => (int?)x.IssueNumber)
                        .MaxAsync() ?? 0;

                var issue = new WarehouseIssue
                {
                    IssueNumber = maxIssueNumber + 1,

                    IssueDate = consumptionDate!.Value,

                    CreatedAt = DateTime.Now,

                    CreatedBy = userName,

                    WarehouseId = warehouse.Id,

                    Status = DocumentStatus.Posted,

                    Source = IssueSource.CartridgeConsumption,

                    EmployeeName = UPS.EmployeeName,

                    Description =
                        $"مصرف باتری برای یو پی اس «{UPS.Name}»"
                };

                _context.WarehouseIssues.Add(issue);

                await _context.SaveChangesAsync();

                // ==========================================
                // 2. ایجاد قلم حواله
                // ==========================================

                var issueItem = new WarehouseIssueItem
                {
                    IssueId = issue.Id,

                    RowNumber = 1,

                    ProductId = ProductId,

                    Quantity = Quantity,

                    Description =
                        $"مصرف برای یو پی اس «{UPS.Name}»"
                };

                _context.WarehouseIssueItems.Add(issueItem);

                await _context.SaveChangesAsync();

                // ==========================================
                // 3. ثبت تراکنش انبار
                // ==========================================

                var inventoryTransaction =
                    new InventoryTransaction
                    {
                        WarehouseId = warehouse.Id,

                        ProductId = ProductId,

                        Quantity = -Quantity,

                        Type = InventoryTransactionType.Issue,

                        TransactionDate = consumptionDate.Value,

                        IssueItemId = issueItem.Id,

                        Description =
                            $"مصرف باتری - حواله شماره {issue.IssueNumber} - یوپی اس {UPS.Name}",

                        CreatedAt = DateTime.Now,

                        CreatedBy = userName
                    };

                _context.InventoryTransactions
                    .Add(inventoryTransaction);

                // ==========================================
                // 4. کاهش موجودی
                // ==========================================

                stock.Quantity -= Quantity;
                stock.UpdatedAt = DateTime.Now;

                // ==========================================
                // 5. ثبت مصرف باتری
                // ==========================================

                var consumption = new BatteryConsumption
                {
                    UPS_Id = UPS.Id,

                    ProductId = ProductId,

                    Quantity = Quantity,

                    ConsumptionDate = consumptionDate.Value,

                    CreatedAt = DateTime.Now,

                    CreatedBy = userName,

                    Description = Description,

                    WarehouseIssueId = issue.Id
                };

                _context.BatteryConsumptions
                    .Add(consumption);

                // ==========================================
                // 6. ذخیره همه تغییرات
                // ==========================================

                await _context.SaveChangesAsync();

                // ==========================================
                // 7. نهایی شدن Transaction
                // ==========================================

                await transaction.CommitAsync();

                TempData["BatterySuccess"] =
                    "مصرف باتری با موفقیت ثبت شد.";

                return RedirectToPage(
                    new { id = UPS.Id });
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "در هنگام ثبت مصرف باتری خطایی رخ داد. هیچ تغییری ثبت نشد.");

                await LoadConsumptions(id);

                return Page();
            }
        }

        // ==========================================
        // UPS
        // ==========================================

        private async Task LoadUPS(int id)
        {
            UPS = await _context.Assets
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // ==========================================
        // باتری ها
        // ==========================================

        private async Task LoadBatteryProducts()
        {
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w =>
                    w.IsITWarehouse &&
                    w.Type == WarehouseType.Main);

            if (warehouse == null)
            {
                BatteryProduct = new List<BatteryProductViewModel>();
                return;
            }

            BatteryProduct = await _context.WarehouseStocks
                .Where(x =>
                    x.WarehouseId == warehouse.Id &&
                    x.Quantity > 0 &&
                    x.Product.Category != null &&
                    x.Product.Category.Type == AssetCategoryType.Consumable &&
                    x.Product.CategoryId==32)
                .Select(x => new BatteryProductViewModel
                {
                    ProductId = x.ProductId,
                    ProductName = x.Product.ProductName!,
                    Quantity = x.Quantity
                })
                .OrderBy(x => x.ProductName)
                .ToListAsync();
        }

        // ==========================================
        // سوابق مصرف
        // ==========================================

        private async Task LoadConsumptions(int UPS_ID)
        {
            Consumptions = await _context.BatteryConsumptions
                .Include(x => x.Product)
                .Where(x => x.UPS_Id == UPS_ID)
                .OrderByDescending(x => x.ConsumptionDate)
                .ToListAsync();
        }


    }
}
