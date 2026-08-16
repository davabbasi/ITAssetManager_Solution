using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;
using ITAssetManager.Convertor;

namespace ITAssetManager.Pages.WarehouseManagements.WarehouseTransfers
{
    public class EditModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public EditModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]public WarehouseTransfer WarehouseTransfer { get; set; } = default!;
        [BindProperty] public List<WarehouseTransferItem> Items { get; set; } = new();

        [BindProperty] public string ShamsiDate { get; set; }
        public SelectList SourceWarehouseList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList DestinationWarehouseList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public List<Product> Products { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WarehouseTransfer =  await _context.WarehouseTransfers
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (WarehouseTransfer == null)
                return NotFound();    
            ShamsiDate = WarehouseTransfer.TransferDate.ToShamsi();

            Items = WarehouseTransfer.Items
                .OrderBy(x => x.RowNumber)
                .ToList();

            await LoadListsAsync();
            return Page();

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadListsAsync();
                return Page();
            }

            var transfer = await _context.WarehouseTransfers
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == WarehouseTransfer.Id);

            if (transfer == null)
                return NotFound();


            // =========================
            // ویرایش هدر
            // =========================

            transfer.TransferNumber = WarehouseTransfer.TransferNumber;
            transfer.SourceWarehouseId = WarehouseTransfer.SourceWarehouseId;
            transfer.DestinationWarehouseId = WarehouseTransfer.DestinationWarehouseId;
            transfer.Description = WarehouseTransfer.Description;
            transfer.TransferDate = (DateTime)ShamsiDate.ToMiladi();
            // =========================
            // حذف اقلام قبلی
            // =========================

            _context.WarehouseTransferItems.RemoveRange(transfer.Items);


            // =========================
            // ثبت اقلام جدید
            // =========================

            if (Items != null)
            {
                int rowNumber = 1;

                foreach (var item in Items)
                {
                    if (item.ProductId <= 0 || item.Quantity <= 0)
                        continue;

                    _context.WarehouseTransferItems.Add(
                        new WarehouseTransferItem
                        {
                            WarehouseTransferId = transfer.Id,
                            RowNumber = rowNumber++,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity
                        });
                }
            }


            await _context.SaveChangesAsync();

            TempData["Success"] = "انتقال بین انباری با موفقیت ویرایش شد.";

            return RedirectToPage(
                "/WarehouseManagements/WarehouseTransfers/Details",
                new { id = transfer.Id });
        }

        private bool WarehouseTransferExists(int id)
        {
            return _context.WarehouseTransfers.Any(e => e.Id == id);
        }

        // =========================
        // لیست‌های فرم
        // =========================

        private async Task LoadListsAsync()
        {
            var warehouses = await _context.Warehouses
                .OrderBy(x => x.WarehouseName)
                .ToListAsync();

            SourceWarehouseList = new SelectList(
                warehouses,
                "Id",
                "WarehouseName",
                WarehouseTransfer.SourceWarehouseId
            );
            DestinationWarehouseList = new SelectList(
                   warehouses,
                   "Id",
                   "WarehouseName",
                   WarehouseTransfer.DestinationWarehouseId
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
