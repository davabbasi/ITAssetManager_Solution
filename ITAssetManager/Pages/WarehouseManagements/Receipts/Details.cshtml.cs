using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;
using ITAssetManager.Convertor;

namespace ITAssetManager.Pages.WarehouseManagements.Receipts
{
    public class DetailsModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public DetailsModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }
        [BindProperty] public string ShamsiDate { get; set; }

        public WarehouseReceipt WarehouseReceipt { get; set; } = default!;
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WarehouseReceipt = await _context.WarehouseReceipts
             .Include(x => x.Warehouse).ThenInclude(w => w.Keeper)
             .Include(x => x.Items)
             .ThenInclude(x => x.Product)
             .FirstOrDefaultAsync(x => x.Id == id);
            ShamsiDate = WarehouseReceipt.ReceiptDate.ToShamsi();
            if (WarehouseReceipt == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostPostAsync(int id)
        {
            var receipt = await _context.WarehouseReceipts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (receipt == null)
                return NotFound();

            if (receipt.Status != DocumentStatus.Draft)
            {
                TempData["Error"] = "این رسید قابل ثبت نهایی نیست.";
                return RedirectToPage(new { id });
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in receipt.Items)
                {
                    var inventoryTransaction = new InventoryTransaction
                    {
                        WarehouseId = receipt.WarehouseId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Type = InventoryTransactionType.Receipt,
                        TransactionDate = receipt.ReceiptDate,
                        ReceiptItemId = item.Id,
                        Description = $"رسید شماره {receipt.ReceiptNumber}",
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "سیستم"
                    };

                    _context.InventoryTransactions.Add(inventoryTransaction);

                    var stock = await _context.WarehouseStocks
                        .FirstOrDefaultAsync(x =>
                            x.WarehouseId == receipt.WarehouseId &&
                            x.ProductId == item.ProductId);

                    if (stock == null)
                    {
                        stock = new WarehouseStock
                        {
                            WarehouseId = receipt.WarehouseId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UpdatedAt = DateTime.Now
                        };

                        _context.WarehouseStocks.Add(stock);
                    }
                    else
                    {
                        stock.Quantity += item.Quantity;
                        stock.UpdatedAt = DateTime.Now;
                    }
                }

                receipt.Status = DocumentStatus.Posted;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["Success"] = "رسید با موفقیت ثبت نهایی شد.";

                return RedirectToPage(
                    "Details",
                    new { id = receipt.Id }
                );

            }

            catch (Exception ) 
            {
              
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "در هنگام ثبت رسید خطایی رخ داد. هیچ اطلاعاتی ثبت نشد."
                );

                return Page();
            }




            return RedirectToPage(new { id });
        }
        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var receipt = await _context.WarehouseReceipts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (receipt == null)
                return NotFound();

            if (receipt.Status != DocumentStatus.Posted)
            {
                TempData["Error"] = "فقط رسید نهایی شده قابل ابطال است.";
                return RedirectToPage(new { id });
            }

            // فعلاً فقط وضعیت را ابطال می‌کنیم
            receipt.Status = DocumentStatus.Cancelled;

            await _context.SaveChangesAsync();

            TempData["Success"] = "رسید با موفقیت ابطال شد.";

            return RedirectToPage(new { id });
        }
    }
}
