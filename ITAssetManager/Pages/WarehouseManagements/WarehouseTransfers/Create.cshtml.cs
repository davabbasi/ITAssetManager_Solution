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

                WarehouseTransfer.TransferDate = transferDate.Value;

                WarehouseTransfer.TransferNumber = TransferNumber;

                WarehouseTransfer.CreatedAt = DateTime.Now;

                WarehouseTransfer.CreatedBy =User.FindFirstValue(ClaimTypes.Name)!;

                WarehouseTransfer.Status = DocumentStatus.Posted;


                _context.WarehouseTransfers.Add(WarehouseTransfer);

                await _context.SaveChangesAsync();


                // --------------------------------
                // 2. ثبت اقلام انتقال
                // --------------------------------

                foreach (var item in Items)
                {
                    item.WarehouseTransferId =
                        WarehouseTransfer.Id;
                }

                _context.WarehouseTransferItems.AddRange(Items);

                await _context.SaveChangesAsync();


                // --------------------------------
                // 3. بررسی موجودی و تغییر موجودی
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


                    // --------------------------------
                    // کم کردن از انبار مبدا
                    // --------------------------------

                    sourceStock.Quantity -= item.Quantity;

                    sourceStock.UpdatedAt = DateTime.Now;


                    // --------------------------------
                    // پیدا کردن موجودی انبار مقصد
                    // --------------------------------

                    var destinationStock =
                        await _context.WarehouseStocks
                            .FirstOrDefaultAsync(x =>
                                x.WarehouseId ==
                                WarehouseTransfer.DestinationWarehouseId
                                &&
                                x.ProductId == item.ProductId);


                    // اگر کالا قبلاً در مقصد وجود نداشته
                    if (destinationStock == null)
                    {
                        destinationStock = new WarehouseStock
                        {
                            WarehouseId =
                                WarehouseTransfer.DestinationWarehouseId,

                            ProductId = item.ProductId,

                            Quantity = item.Quantity,

                            UpdatedAt = DateTime.Now
                        };

                        _context.WarehouseStocks.Add(
                            destinationStock
                        );
                    }
                    else
                    {
                        // اضافه کردن به موجودی مقصد
                        destinationStock.Quantity += item.Quantity;

                        destinationStock.UpdatedAt = DateTime.Now;
                    }


                    // --------------------------------
                    // ثبت TransferOut
                    // --------------------------------

                    var transferOut =
                        new InventoryTransaction
                        {
                            WarehouseId =
                                WarehouseTransfer.SourceWarehouseId,

                            ProductId = item.ProductId,

                            Quantity = item.Quantity,

                            Type =
                                InventoryTransactionType.TransferOut,

                            TransactionDate =
                                WarehouseTransfer.TransferDate,

                            CreatedAt = DateTime.Now,

                            CreatedBy =WarehouseTransfer.CreatedBy,
                            TransferItemId=item.Id,
                        };

                    _context.InventoryTransactions.Add(
                        transferOut
                    );


                    // --------------------------------
                    // ثبت TransferIn
                    // --------------------------------

                    var transferIn =
                        new InventoryTransaction
                        {
                            WarehouseId =
                                WarehouseTransfer.DestinationWarehouseId,

                            ProductId = item.ProductId,

                            Quantity = item.Quantity,

                            Type =
                                InventoryTransactionType.TransferIn,

                            TransactionDate =
                                WarehouseTransfer.TransferDate,

                            CreatedAt = DateTime.Now,

                            CreatedBy =
                                WarehouseTransfer.CreatedBy
                        };

                    _context.InventoryTransactions.Add(
                        transferIn
                    );
                }


                // --------------------------------
                // 4. ذخیره تغییرات موجودی
                // --------------------------------

                await _context.SaveChangesAsync();


                // --------------------------------
                // 5. همه چیز موفق بود
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
                await _context.Warehouses
                    .OrderBy(x => x.WarehouseName)
                    .ToListAsync(),
                "Id",
                "WarehouseName"
            );


            Products = await _context.Products
                .OrderBy(x => x.ProductName)
                .ToListAsync();
        }
    }
}
