using System.Security.Claims;
using ITAssetManager.Convertor;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Printers
{
    [Authorize]
    public class CartridgeModel : PageModel
    {

        private readonly ApplicationDbContext _context;

        public CartridgeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Asset Printer { get; set; } = null!;

        public List<CartridgeConsumption> Consumptions { get; set; } = new();

        // -----------------------------
        // اطلاعات فرم مصرف کارتریج
        // -----------------------------

        [BindProperty]
        public int ProductId { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        [BindProperty]
        public string ConsumptionShamsiDate { get; set; } = string.Empty;

        [BindProperty]
        public string? Description { get; set; }

        public class CartridgeProductViewModel
        {
            public int ProductId { get; set; }

            public string ProductName { get; set; } = string.Empty;

            public decimal Quantity { get; set; }
        }

        public List<CartridgeProductViewModel> CartridgeProducts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await LoadPrinter(id);

            if (Printer == null)
                return NotFound();

            await LoadCartridgeProducts();
            await LoadConsumptions(id);

            ConsumptionShamsiDate = DateTime.Now.ToShamsi();

            return Page();
        }

        // ==========================================
        // ثبت مصرف کارتریج
        // ==========================================

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await LoadPrinter(id);

            if (Printer == null)
                return NotFound();

            await LoadCartridgeProducts();

            // -----------------------------
            // اعتبارسنجی اولیه
            // -----------------------------

            if (ProductId <= 0)
            {
                ModelState.AddModelError(
                    nameof(ProductId),
                    "لطفاً کارتریج را انتخاب کنید.");
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
                    $"موجودی کارتریج «{stock?.Product?.ProductName ?? "انتخاب شده"}» کافی نیست.");

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

                    Description =
                        $"مصرف کارتریج برای چاپگر «{Printer.Name}»"
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
                        $"مصرف برای چاپگر «{Printer.Name}»"
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
                            $"مصرف کارتریج - حواله شماره {issue.IssueNumber} - چاپگر {Printer.Name}",

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
                // 5. ثبت مصرف کارتریج
                // ==========================================

                var consumption = new CartridgeConsumption
                {
                    PrinterId = Printer.Id,

                    ProductId = ProductId,

                    Quantity = Quantity,

                    ConsumptionDate = consumptionDate.Value,

                    CreatedAt = DateTime.Now,

                    CreatedBy = userName,

                    Description = Description,

                    WarehouseIssueId = issue.Id
                };

                _context.CartridgeConsumptions
                    .Add(consumption);

                // ==========================================
                // 6. ذخیره همه تغییرات
                // ==========================================

                await _context.SaveChangesAsync();

                // ==========================================
                // 7. نهایی شدن Transaction
                // ==========================================

                await transaction.CommitAsync();

                TempData["CartridgeSuccess"] =
                    "مصرف کارتریج با موفقیت ثبت شد.";

                return RedirectToPage(
                    new { id = Printer.Id });
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "در هنگام ثبت مصرف کارتریج خطایی رخ داد. هیچ تغییری ثبت نشد.");

                await LoadConsumptions(id);

                return Page();
            }
        }

        // ==========================================
        // چاپگر
        // ==========================================

        private async Task LoadPrinter(int id)
        {
            Printer = await _context.Assets
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // ==========================================
        // کارتریج‌ها
        // ==========================================

        private async Task LoadCartridgeProducts()
        {
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w =>
                    w.IsITWarehouse &&
                    w.Type == WarehouseType.Main);

            if (warehouse == null)
            {
                CartridgeProducts = new List<CartridgeProductViewModel>();
                return;
            }

            CartridgeProducts = await _context.WarehouseStocks
                .Where(x =>
                    x.WarehouseId == warehouse.Id &&
                    x.Quantity > 0 &&
                    x.Product.Category != null &&
                    x.Product.Category.Type == AssetCategoryType.Consumable)
                .Select(x => new CartridgeProductViewModel
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

        private async Task LoadConsumptions(int printerId)
        {
            Consumptions = await _context.CartridgeConsumptions
                .Include(x => x.Product)
                .Where(x => x.PrinterId == printerId)
                .OrderByDescending(x => x.ConsumptionDate)
                .ToListAsync();
        }


    }
}

