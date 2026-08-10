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

namespace ITAssetManager.Pages.WarehouseManagements.Receipts
{
    public class CreateModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;      
        public CreateModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]public WarehouseReceipt WarehouseReceipt { get; set; } = new();
        [BindProperty] public List<WarehouseReceiptItem> Items { get; set; } = new();
        public SelectList WarehouseList { get; set; } = null!;
        public SelectList PurchaseList { get; set; } = null!;
        public List<Product> Products { get; set; } = new();
        [BindProperty] public string ShamsiDate { get; set; }
        [BindProperty] public int ReceiptNumber { get; set; }

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

            var receiptDate = ShamsiDate.ToMiladi();

            if (!receiptDate.HasValue)
            {
                ModelState.AddModelError(
                    "WarehouseReceipt.ReceiptDate",
                    "لطفاً تاریخ رسید را به‌درستی وارد کنید."
                );

                await LoadLists();
                return Page();
            }

            var maxReceiptNumber = await _context.WarehouseReceipts.Select(r => (int?)r.ReceiptNumber).MaxAsync() ?? 0;
            ReceiptNumber = maxReceiptNumber + 1;

            WarehouseReceipt.ReceiptNumber = ReceiptNumber;
            WarehouseReceipt.ReceiptDate = receiptDate.Value;
            WarehouseReceipt.CreatedAt = DateTime.Now;
            WarehouseReceipt.CreatedBy = User.Identity?.Name ?? "سیستم";
            WarehouseReceipt.Status = DocumentStatus.Draft;

            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].RowNumber = i + 1;
            }

            WarehouseReceipt.Items = Items;

            _context.WarehouseReceipts.Add(WarehouseReceipt);

            await _context.SaveChangesAsync();

            TempData["Success"] = "رسید با موفقیت ذخیره شد.";

            return RedirectToPage("./Details", new
            {
                id = WarehouseReceipt.Id
            });
        }


        

        private async Task LoadLists()
        {
            var maxReceiptNumber = await _context.WarehouseReceipts.Select(r => (int?)r.ReceiptNumber).MaxAsync() ?? 0;
            ReceiptNumber = maxReceiptNumber + 1;
            WarehouseList = new SelectList(
                await _context.Warehouses
                    .OrderBy(x => x.WarehouseName)
                    .ToListAsync(),
                "Id",
                "WarehouseName");
            Products = await _context.Products
                .OrderBy(x => x.ProductName)
                .ToListAsync();

            PurchaseList = new SelectList(
                await _context.VwPurchaseRequests
                    .OrderByDescending(x => x.RequestNo)
                    .ToListAsync(),
                "RequestNo",
                "RequestNo");
        }
    }
}
