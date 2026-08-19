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

namespace ITAssetManager.Pages.WarehouseManagements.WarehouseTransfers
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WarehouseTransfer WarehouseTransfer { get; set; } = new();

        [BindProperty]
        public List<WarehouseTransferItem> Items { get; set; } = new();

        [BindProperty]
        public string ShamsiDate { get; set; } = string.Empty;

        public int TransferNumber { get; set; }

        public SelectList WarehouseList { get; set; } = null!;

        public List<Product> Products { get; set; } = new();
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadLists();

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Items.WarehouseTransfer");

            if (!ModelState.IsValid)
            {
                await LoadLists();
                return Page();
            }


            // بررسی تاریخ
            var transferDate = ShamsiDate.ToMiladi();

            if (!transferDate.HasValue)
            {
                ModelState.AddModelError(
                    "WarehouseTransfer.TransferDate",
                    "لطفاً تاریخ انتقال را به‌درستی وارد کنید."
                );

                await LoadLists();
                return Page();
            }


            // بررسی مبدا و مقصد
            if (WarehouseTransfer.SourceWarehouseId ==
                WarehouseTransfer.DestinationWarehouseId)
            {
                ModelState.AddModelError(
                    "",
                    "انبار مبدا و مقصد نمی‌توانند یکسان باشند."
                );

                await LoadLists();
                return Page();
            }


            // بررسی اینکه حداقل یک قلم وجود دارد
            if (Items == null || Items.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "حداقل یک کالا برای انتقال وارد کنید."
                );

                await LoadLists();
                return Page();
            }


            // شروع Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // --------------------------------
                // 1. اطلاعات سند انتقال
                // --------------------------------
                var maxTransferNumber =
               await _context.WarehouseTransfers
                   .Select(x => (int?)x.TransferNumber)
                   .MaxAsync() ?? 0;
                TransferNumber = maxTransferNumber + 1;

                WarehouseTransfer.TransferNumber = TransferNumber;
                WarehouseTransfer.TransferDate = transferDate.Value;
                WarehouseTransfer.TransferNumber = TransferNumber;
                WarehouseTransfer.CreatedAt = DateTime.Now;
                WarehouseTransfer.CreatedBy =User.FindFirstValue(ClaimTypes.Name)!;
                WarehouseTransfer.Status = DocumentStatus.Draft;
                _context.WarehouseTransfers.Add(WarehouseTransfer);
                await _context.SaveChangesAsync();

                // --------------------------------
                // 2. بررسی موجودی و ثبت اقلام انتقال
                // --------------------------------

                foreach (var item in Items)
                {
                    // پیدا کردن موجودی انبار مبدا
                    var sourceStock =
                        await _context.WarehouseStocks
                            .FirstOrDefaultAsync(x =>
                                x.WarehouseId ==
                                WarehouseTransfer.SourceWarehouseId
                                &&
                                x.ProductId == item.ProductId);


                    // اگر موجودی وجود نداشت
                    if (sourceStock == null)
                    {
                        throw new Exception(
                            $"برای کالا با شناسه {item.ProductId} در انبار مبدا موجودی ثبت نشده است."
                        );
                    }


                    // بررسی موجودی
                    if (sourceStock.Quantity < item.Quantity)
                    {
                        throw new Exception(
                            $"موجودی کالای {item.ProductId} در انبار مبدا کافی نیست."
                        );
                    }


                    //  ثبت اقلام انتقال

                    item.WarehouseTransferId = WarehouseTransfer.Id;
                    _context.WarehouseTransferItems.AddRange(Items);
                    await _context.SaveChangesAsync();

                }


                // --------------------------------
                // 3. همه چیز موفق بود
                // --------------------------------

                await transaction.CommitAsync();
                return RedirectToPage(
                    "Details",
                    new { id = WarehouseTransfer.Id }
                );
            }
            catch (Exception ex)
            {
                // اگر هر مرحله‌ای خطا داد
                // همه چیز Rollback می‌شود

                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "انتقال کالا انجام نشد: " + ex.Message
                );

                await LoadLists();

                return Page();
            }
        }
        private async Task LoadLists()
        {
            var maxTransferNumber =
                await _context.WarehouseTransfers
                    .Select(x => (int?)x.TransferNumber)
                    .MaxAsync() ?? 0;

            TransferNumber = maxTransferNumber + 1;


            WarehouseList = new SelectList(
                await _context.Warehouses.Where(w=>w.IsITWarehouse&&w.Type==WarehouseType.Main)
                    .OrderBy(x => x.WarehouseName)
                    .ToListAsync(),
                "Id",
                "WarehouseName"
            );


            Products = await _context.Products
                .OrderBy(x => x.ProductName)
                .ToListAsync();
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
