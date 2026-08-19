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
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.WarehouseManagements.WarehouseTransfers
{
    public class DetailsModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public DetailsModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public WarehouseTransfer WarehouseTransfer { get; set; } = default!;
        [BindProperty] public string ShamsiDate { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WarehouseTransfer = await _context.WarehouseTransfers
                .Include(t=>t.Items)
                .ThenInclude(t => t.Product)
                .Include(t => t.SourceWarehouse)
                .Include(t=>t.DestinationWarehouse)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (WarehouseTransfer == null)
            {
                return NotFound();
            }
           

            ShamsiDate = WarehouseTransfer.TransferDate.ToShamsi();

          
            return Page();
        }
        public async Task<IActionResult> OnPostPostAsync(int id)
        {
            var transfer = await _context.WarehouseTransfers
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (transfer == null)
                return NotFound();

            if (transfer.Status != DocumentStatus.Draft)
            {
                TempData["TransferError"] = "این انتقال قابل ثبت نهایی نیست.";
                return RedirectToPage(new { id });
            }
            // شروع Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
               
                // --------------------------------
                // 1. بررسی موجودی و تغییر موجودی
                // --------------------------------

                foreach (var item in transfer.Items)
                {
                    // پیدا کردن موجودی انبار مبدا
                    var sourceStock =
                        await _context.WarehouseStocks
                            .FirstOrDefaultAsync(x =>
                                x.WarehouseId ==
                                transfer.SourceWarehouseId
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
                                transfer.DestinationWarehouseId
                                &&
                                x.ProductId == item.ProductId);


                    // اگر کالا قبلاً در مقصد وجود نداشته
                    if (destinationStock == null)
                    {
                        destinationStock = new WarehouseStock
                        {
                            WarehouseId =
                                transfer.DestinationWarehouseId,

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
                            WarehouseId =transfer.SourceWarehouseId,
                            ProductId = item.ProductId,
                            Quantity = -item.Quantity,
                            Type =InventoryTransactionType.TransferOut,
                            TransactionDate = transfer.TransferDate,
                            CreatedAt = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "سیستم",
                            TransferItemId = item.Id,
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
                            WarehouseId =transfer.DestinationWarehouseId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Type =InventoryTransactionType.TransferIn,
                            TransactionDate =transfer.TransferDate,
                            CreatedAt = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "سیستم",
                            TransferItemId = item.Id,
                        };
                    _context.InventoryTransactions.Add(transferIn);
                }


                // --------------------------------
                // 4. ذخیره تغییرات موجودی
                // --------------------------------
                transfer.Status=DocumentStatus.Posted;
                await _context.SaveChangesAsync();


                // --------------------------------
                // 5. همه چیز موفق بود
                // --------------------------------

                await transaction.CommitAsync();


                return RedirectToPage(
                    "Details",
                    new { id = transfer.Id }
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

                return Page();
            }
        }
        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var transfer = await _context.WarehouseTransfers
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (transfer == null)
                return NotFound();

            if (transfer.Status != DocumentStatus.Posted)
            {
                TempData["TransferCancelError"] = "فقط انتقال نهایی شده قابل ابطال است.";
                return RedirectToPage(new { id });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in transfer.Items)
                {
                    // پیدا کردن موجودی فعلی در انبار مبدا انتقال بین انباری مد نظر
                    var sourceWarehouseStock = await _context.WarehouseStocks
                        .FirstOrDefaultAsync(x =>
                            x.WarehouseId == transfer.SourceWarehouseId &&
                            x.ProductId == item.ProductId);
                    if (sourceWarehouseStock == null)
                    {
                        sourceWarehouseStock = new WarehouseStock
                        {
                            WarehouseId = transfer.SourceWarehouseId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UpdatedAt = DateTime.Now
                        };

                        _context.WarehouseStocks.Add(sourceWarehouseStock);
                    }
                    else
                    {
                        sourceWarehouseStock.Quantity += item.Quantity;
                        sourceWarehouseStock.UpdatedAt = DateTime.Now;
                    }

                    // پیدا کردن موجودی فعلی در انبار مقصد انتقال بین انباری مد نظر
                    var destinationWarehouseStock = await _context.WarehouseStocks
                        .FirstOrDefaultAsync(x =>
                            x.WarehouseId == transfer.DestinationWarehouseId &&
                            x.ProductId == item.ProductId);
                    if (destinationWarehouseStock == null || destinationWarehouseStock.Quantity < item.Quantity || destinationWarehouseStock.Quantity ==0)
                    {
                        await transaction.RollbackAsync();

                        TempData["TransferCancelError"] =
                            $"موجودی انبار {transfer.DestinationWarehouse.WarehouseName}جهت ابطال انتقال کافی نیست . هیچ تغییری اعمال نشد.";

                        return RedirectToPage(new { id });
                    }
                    else
                    {
                        destinationWarehouseStock.Quantity -= item.Quantity;
                        destinationWarehouseStock.UpdatedAt = DateTime.Now;
                    }

                    // --------------------------------
                    //  ثبت تراکنش معکوس برای انبار مبدا
                    // --------------------------------

                    var reverseSourceWarehouseTransaction = new InventoryTransaction
                    {
                        WarehouseId = transfer.SourceWarehouseId,
                        ProductId = item.ProductId,

                        // مقدار مثبت چون داریم اثر خروج کالا از انبار مبدا را برمی‌گردانیم
                        Quantity = +item.Quantity,

                        Type = InventoryTransactionType.AdjustmentIn,

                        TransactionDate = DateTime.Now,

                        TransferItemId = item.Id,

                        Description =
                            $"ابطال انتقال شماره {transfer.TransferNumber}",

                        CreatedAt = DateTime.Now,

                        CreatedBy = User.Identity?.Name ?? "سیستم"
                    };
                    _context.InventoryTransactions.Add(reverseSourceWarehouseTransaction);

                    var reverseDestinationWarehouseTransaction = new InventoryTransaction
                    {
                        WarehouseId = transfer.DestinationWarehouseId,
                        ProductId = item.ProductId,

                        // مقدار منفی چون داریم اثر ورود به انبار مقصد را برمی‌گردانیم
                        Quantity = -item.Quantity,

                        Type = InventoryTransactionType.AdjustmentOut,

                        TransactionDate = DateTime.Now,

                        TransferItemId = item.Id,

                        Description =
                           $"ابطال انتقال شماره {transfer.TransferNumber}",

                        CreatedAt = DateTime.Now,

                        CreatedBy = User.Identity?.Name ?? "سیستم"
                    };


                    _context.InventoryTransactions.Add(reverseDestinationWarehouseTransaction);
                }


                // --------------------------------
                //  تغییر وضعیت انتقال
                // --------------------------------

                transfer.Status = DocumentStatus.Cancelled;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["TransferCancelSuccess"] =
                    "انتقال بین انباری با موفقیت ابطال شد و اثر آن از موجودی برگشت داده شد.";

                return RedirectToPage(new { id = transfer.Id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                TempData["TransferCancelError"] =
                    "در هنگام ابطال انتقال بین انباری خطایی رخ داد. هیچ تغییری اعمال نشد.";

                return RedirectToPage(new { id });
            }
        }
    }
}
