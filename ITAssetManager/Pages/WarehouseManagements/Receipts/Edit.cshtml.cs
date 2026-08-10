using ITAssetManager.Convertor;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.WarehouseManagements.Receipts;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public WarehouseReceipt WarehouseReceipt { get; set; } = new();

    [BindProperty] public List<WarehouseReceiptItem> Items { get; set; } = new();

    public SelectList WarehouseList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

    public SelectList KeeperList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

    public List<Product> Products { get; set; } = new();

    [BindProperty] public string ShamsiDate { get; set; }

    // =========================
    // GET
    // =========================

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var receipt = await _context.WarehouseReceipts
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (receipt == null)
            return NotFound();

        WarehouseReceipt = receipt;
        ShamsiDate = WarehouseReceipt.ReceiptDate.ToShamsi();
        Items = receipt.Items
            .OrderBy(x => x.RowNumber)
            .ToList();

        await LoadListsAsync();

        return Page();
    }


    // =========================
    // POST
    // =========================

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadListsAsync();
            return Page();
        }

        var receipt = await _context.WarehouseReceipts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == WarehouseReceipt.Id);

        if (receipt == null)
            return NotFound();


        // =========================
        // ویرایش هدر
        // =========================

        receipt.ReceiptNumber = WarehouseReceipt.ReceiptNumber;
        receipt.WarehouseId = WarehouseReceipt.WarehouseId;
        receipt.ReferenceNumber = WarehouseReceipt.ReferenceNumber;
        receipt.Description = WarehouseReceipt.Description;
        receipt.ReceiptDate = (DateTime)ShamsiDate.ToMiladi();
        // =========================
        // حذف اقلام قبلی
        // =========================

        _context.WarehouseReceiptItems.RemoveRange(receipt.Items);


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

                _context.WarehouseReceiptItems.Add(
                    new WarehouseReceiptItem
                    {
                        ReceiptId = receipt.Id,
                        RowNumber = rowNumber++,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    });
            }
        }


        await _context.SaveChangesAsync();

        TempData["Success"] = "رسید انبار با موفقیت ویرایش شد.";

        return RedirectToPage(
            "/WarehouseManagements/Receipts/Details",
            new { id = receipt.Id });
    }


    // =========================
    // لیست‌های فرم
    // =========================

    private async Task LoadListsAsync()
    {
        var warehouses = await _context.Warehouses
            .OrderBy(x => x.WarehouseName)
            .ToListAsync();

        WarehouseList = new SelectList(
            warehouses,
            "Id",
            "WarehouseName",
            WarehouseReceipt.WarehouseId
        );


        var keepers = await _context.VwEmployees
            .OrderBy(x => x.FullName)
            .Select(x => new
            {
                x.Id,
                Name = x.FullName
            })
            .ToListAsync();


        Products = await _context.Products
            .OrderBy(x => x.ProductName)
            .ToListAsync();

    }
}