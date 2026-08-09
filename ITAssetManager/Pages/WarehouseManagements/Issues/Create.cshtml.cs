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

        public SelectList WarehouseList { get; set; } = null!;
        public List<Product> Products { get; set; } = new();
        [BindProperty] public string ShamsiDate { get; set; }
        [BindProperty] public WarehouseIssue WarehouseIssue { get; set; } = default!;
         public int IssueNumber { get; set; }

        public class ProductStockDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int Stock { get; set; }
        }

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
            WarehouseIssue.IssueDate = issueDate.Value;
            WarehouseIssue.CreatedBy = User.FindFirstValue(ClaimTypes.Name)!;

            _context.WarehouseIssues.Add(WarehouseIssue);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task LoadLists()
        {
            var maxIssueNumber = await _context.WarehouseIssues.Select(r => (int?)r.IssueNumber).MaxAsync() ?? 0;
            IssueNumber = maxIssueNumber + 1;

            WarehouseList = new SelectList(
                await _context.Warehouses
                    .OrderBy(x => x.WarehouseName)
                    .ToListAsync(),
                "Id",
                "WarehouseName");
            Products = await _context.Products
                .OrderBy(x => x.ProductName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetAvailableProductsAsync(int warehouseId)
        {
            return await _inventoryService.OnGetAvailableProductsAsync(warehouseId);
        }


    }
}
