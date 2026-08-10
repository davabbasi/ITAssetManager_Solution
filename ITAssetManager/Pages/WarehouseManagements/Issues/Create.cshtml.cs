using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ITAssetManager.Convertor;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Services;

namespace ITAssetManager.Pages.WarehouseManagements.Issues
{
    public class CreateModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;
        private readonly InventoryService _inventoryService;
        public CreateModel(ApplicationDbContext context,InventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }
        public SelectList EmployeeList { get; set; } = null!;
        public SelectList WarehouseList { get; set; } = null!;
        [BindProperty] public string ShamsiDate { get; set; }
        [BindProperty] public WarehouseIssue WarehouseIssue { get; set; } = default!;
         public int IssueNumber { get; set; }
        [BindProperty] public List<WarehouseIssueItem> Items { get; set; } = new();



        public async Task< IActionResult> OnGet()
        {
            await LoadLists();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadLists();
                return Page();
            }
            var maxIssueNumber = await _context.WarehouseIssues
                .Select(x => (int?)x.IssueNumber)
                .MaxAsync() ?? 0;
            IssueNumber = maxIssueNumber + 1;

            var issueDate = ShamsiDate.ToMiladi();
            if (!issueDate.HasValue)
            {
                ModelState.AddModelError(
                    "WarehouseIssue.IssueDate",
                    "لطفاً تاریخ حواله را به‌درستی وارد کنید."
                );

                await LoadLists();
                return Page();
            }

            // شروع Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // --------------------------------
                // 1. تکمیل اطلاعات حواله
                // --------------------------------

                WarehouseIssue.IssueDate = issueDate.Value;
                WarehouseIssue.CreatedAt = DateTime.Now;
                WarehouseIssue.CreatedBy = User.FindFirstValue(ClaimTypes.Name)!;
                WarehouseIssue.IssueNumber = IssueNumber;

                // ثبت حواله
                _context.WarehouseIssues.Add(WarehouseIssue);
                await _context.SaveChangesAsync();


                // --------------------------------
                // 2. ثبت اقلام حواله
                // --------------------------------

                foreach (var item in Items)
                {
                    item.IssueId = WarehouseIssue.Id;
                }

                _context.WarehouseIssueItems.AddRange(Items);
                await _context.SaveChangesAsync();


                // --------------------------------
                // 3. ثبت گردش موجودی و افزایش موجودی
                // --------------------------------

                foreach (var item in Items)
                {
                    // پیدا کردن موجودی فعلی کالا در این انبار
                    var stock = await _context.WarehouseStocks
                        .FirstOrDefaultAsync(x =>
                            x.WarehouseId == WarehouseIssue.WarehouseId &&
                            x.ProductId == item.ProductId);

                    // اگر موجودی وجود نداشت
                    if (stock == null)
                    {
                        throw new Exception(
                            $"برای کالا با شناسه {item.ProductId} در انبار مبدا موجودی ثبت نشده است."
                        );
                    }


                    // بررسی موجودی
                    if (stock.Quantity < item.Quantity)
                    {
                        throw new Exception(
                            $"موجودی کالای {item.ProductId} در انبار مبدا کافی نیست."
                        );
                    }


                    stock.Quantity -= item.Quantity;
                    stock.UpdatedAt = DateTime.Now;


                    // ثبت گردش کالا
                    var transactionItem = new InventoryTransaction
                    {
                        WarehouseId = WarehouseIssue.WarehouseId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Type = InventoryTransactionType.Issue,
                        IssueItemId = item.Id,
                        TransactionDate = WarehouseIssue.IssueDate,
                        Description = $"حواله شماره {WarehouseIssue.IssueNumber}",
                        CreatedBy = User.FindFirstValue(ClaimTypes.Name)!
                    };

                    _context.InventoryTransactions.Add(transactionItem);
                }

                await _context.SaveChangesAsync();


                // --------------------------------
                // 4. همه چیز موفق بود
                // --------------------------------

                await transaction.CommitAsync();

                return RedirectToPage(
                    "Details",
                    new { id = WarehouseIssue.Id }
                );
            }
            catch (Exception)
            {
                // اگر هر مرحله‌ای خطا داد
                // تمام عملیات برگردانده می‌شود
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "در هنگام ثبت حواله خطایی رخ داد. هیچ اطلاعاتی ثبت نشد."
                );

                await LoadLists();
                return Page();
            }
        }

        private async Task LoadLists()
        {
            var maxIssueNumber = await _context.WarehouseIssues.Select(r => (int?)r.IssueNumber).MaxAsync() ?? 0;
            IssueNumber = maxIssueNumber + 1;

            EmployeeList = new SelectList(
                await _context.VwEmployees
              .OrderBy(e => e.FullName)
              .Select(e => new { e.Id, Name = e.FullName + " - " + e.DepartmentName })
              .ToListAsync(), "Id", "Name");

            WarehouseList = new SelectList(
                await _context.Warehouses
                    .OrderBy(x => x.WarehouseName)
                    .ToListAsync(),
                "Id",
                "WarehouseName");
        }

       
        public async Task<IActionResult> OnGetWarehouseStockAsync(int warehouseId)
        {
            var stocks = await _context.WarehouseStocks
                .Where(x => x.WarehouseId == warehouseId && x.Quantity > 0)
                .Include(x => x.Product)
                .OrderBy(x => x.Product.ProductName)
                .Select(x => new
                {
                    productId = x.ProductId,
                    productName = x.Product.ProductName,
                    quantity = x.Quantity
                })
                .ToListAsync();

            return new JsonResult(stocks);
        }

    }
}
