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

namespace ITAssetManager.Pages.WarehouseManagements.Issues
{
    public class DetailsModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;
        public DetailsModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }
        public WarehouseIssue WarehouseIssue { get; set; } = default!;
        [BindProperty] public string ShamsiDate { get; set; }=string.Empty;
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WarehouseIssue = await _context.WarehouseIssues
                .Include(x => x.Warehouse).ThenInclude(w => w.Keeper)
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
             .FirstOrDefaultAsync(x => x.Id == id);

            ShamsiDate = WarehouseIssue.IssueDate.ToShamsi();

            if (WarehouseIssue == null)
            {
                return NotFound();
            }

            return Page();
        }
        public async Task<IActionResult> OnPostPostAsync(int id)
        {
            var issue = await _context.WarehouseIssues
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (issue == null)
                return NotFound();

            if (issue.Status != DocumentStatus.Draft)
            {
                TempData["IssuedError"] = "این حواله قابل ثبت نهایی نیست.";
                return RedirectToPage(new { id });
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in issue.Items)
                {
                    var inventoryTransaction = new InventoryTransaction
                    {
                        WarehouseId = issue.WarehouseId,
                        ProductId = item.ProductId,
                        Quantity = -item.Quantity,
                        Type = InventoryTransactionType.Issue,
                        TransactionDate = issue.IssueDate,
                        IssueItemId = item.Id,
                        Description = $"حواله شماره {issue.IssueNumber}",
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "سیستم"
                    };

                    _context.InventoryTransactions.Add(inventoryTransaction);

                    var stock = await _context.WarehouseStocks
                        .FirstOrDefaultAsync(x =>
                            x.WarehouseId == issue.WarehouseId &&
                            x.ProductId == item.ProductId);

                    if (stock == null)
                    {
                        throw new Exception(
                            $"موجودی کالا با شناسه {item.ProductId} در انبار پیدا نشد."
                        );
                    }
                    // بررسی اینکه موجودی برای ثبت حواله کافی باشد
                    if (stock.Quantity < item.Quantity)
                    {
                        TempData["Error"] =
                            $"امکان ثبت حواله وجود ندارد؛ موجودی کالا «{item.Product?.ProductName}» کافی نیست.";

                        await transaction.RollbackAsync();

                        return RedirectToPage(new { id });
                    }

                    // --------------------------------
                    //  کاهش موجودی
                    // --------------------------------

                    stock.Quantity -= item.Quantity;
                    stock.UpdatedAt = DateTime.Now;
                }

                issue.Status = DocumentStatus.Posted;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["IssuedSuccess"] = "حواله با موفقیت ثبت نهایی شد.";

                return RedirectToPage(
                    "Details",
                    new { id = issue.Id }
                );

            }

            catch (Exception)
            {

                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "در هنگام ثبت حواله خطایی رخ داد. هیچ اطلاعاتی ثبت نشد."
                );

                return Page();
            }
        }
        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var issue = await _context.WarehouseIssues
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (issue == null)
                return NotFound();

            if (issue.Status != DocumentStatus.Posted)
            {
                TempData["IssueCancelError"] = "فقط حواله نهایی شده قابل ابطال است.";
                return RedirectToPage(new { id });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in issue.Items)
                {
                    // پیدا کردن موجودی فعلی
                    var stock = await _context.WarehouseStocks
                        .FirstOrDefaultAsync(x =>
                            x.WarehouseId == issue.WarehouseId &&
                            x.ProductId == item.ProductId);
                    if (stock == null)
                    {
                        stock = new WarehouseStock
                        {
                            WarehouseId = issue.WarehouseId,
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
                 


                    // --------------------------------
                    //  ثبت تراکنش معکوس
                    // --------------------------------

                    var reverseTransaction = new InventoryTransaction
                    {
                        WarehouseId = issue.WarehouseId,
                        ProductId = item.ProductId,

                        // مقدار مثبت چون داریم اثر حواله را برمی‌گردانیم
                        Quantity = +item.Quantity,

                        Type = InventoryTransactionType.AdjustmentIn,

                        TransactionDate = DateTime.Now,

                        IssueItemId = item.Id,

                        Description =
                            $"ابطال حواله شماره {issue.IssueNumber}",

                        CreatedAt = DateTime.Now,

                        CreatedBy = User.Identity?.Name ?? "سیستم"
                    };

                    _context.InventoryTransactions.Add(reverseTransaction);
                }


                // --------------------------------
                //  تغییر وضعیت حواله
                // --------------------------------

                issue.Status = DocumentStatus.Cancelled;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["IssueCancelSuccess"] =
                    "حواله با موفقیت ابطال شد و اثر آن از موجودی برگشت داده شد.";

                return RedirectToPage(new { id = issue.Id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                TempData["IssueCancelError"] =
                    "در هنگام ابطال حواله خطایی رخ داد. هیچ تغییری اعمال نشد.";

                return RedirectToPage(new { id });
            }
        }
    }
}
