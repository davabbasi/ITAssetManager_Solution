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
        [BindProperty] public WarehouseIssue WarehouseIssue { get; set; } = default!;
        [BindProperty] public List<WarehouseIssueItem> Items { get; set; } = new();
        public SelectList WarehouseList { get; set; } = null!;
        public List<Product> Products { get; set; } = new();
        [BindProperty] public string ShamsiDate { get; set; } = string.Empty;
        public int IssueNumber { get; set; }
        public SelectList EmployeeList { get; set; } = null!;

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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. تکمیل هدر
                WarehouseIssue.IssueDate = issueDate.Value;
                WarehouseIssue.CreatedAt = DateTime.Now;
                WarehouseIssue.CreatedBy = User.FindFirstValue(ClaimTypes.Name)!;
                WarehouseIssue.IssueNumber = IssueNumber;
                WarehouseIssue.Status = DocumentStatus.Draft;

                WarehouseIssue.EmployeeName = await _context.VwEmployees
                    .Where(e => e.Id == WarehouseIssue.EmployeeId)
                    .Select(e => e.FullName)
                    .FirstOrDefaultAsync();

                _context.WarehouseIssues.Add(WarehouseIssue);

                await _context.SaveChangesAsync();


                // 2. بررسی موجودی تمام اقلام
                foreach (var item in Items)
                {
                    var stock = await _context.WarehouseStocks
                        .FirstOrDefaultAsync(x =>
                            x.WarehouseId == WarehouseIssue.WarehouseId &&
                            x.ProductId == item.ProductId);

                    if (stock == null || stock.Quantity < item.Quantity)
                    {
                        ModelState.AddModelError(
                            "",
                            $"موجودی برخی از کالاهای انتخاب شده کافی نیست."
                        );

                        await transaction.RollbackAsync();
                        ModelState.Remove("WarehouseIssue.WarehouseId");
                        WarehouseIssue.WarehouseId = 0;

                        await LoadLists();
                        return Page();
                    }
                }


                // 3. ثبت اقلام
                foreach (var item in Items)
                {
                    item.IssueId = WarehouseIssue.Id;
                }

                _context.WarehouseIssueItems.AddRange(Items);

                await _context.SaveChangesAsync();


                // 4. تأیید نهایی Transaction
                await transaction.CommitAsync();

                TempData["IssueDraftedSuccess"] =
                    "پیش‌نویس حواله با موفقیت ایجاد شد.";

                return RedirectToPage("./Details", new
                {
                    id = WarehouseIssue.Id
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "در هنگام ایجاد پیش‌نویس حواله خطایی رخ داد. هیچ اطلاعاتی ثبت نشد."
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
