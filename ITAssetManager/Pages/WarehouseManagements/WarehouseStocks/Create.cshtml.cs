using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ITAssetManager.Data;
using ITAssetManager.Models;

namespace ITAssetManager.Pages.WarehouseManagements.WarehouseStocks
{
    public class CreateModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public CreateModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["ProductId"] = new SelectList(_context.Products, "Id", "ProductName");
        ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "WarehouseName");
            return Page();
        }

        [BindProperty]
        public WarehouseStock WarehouseStock { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.WarehouseStocks.Add(WarehouseStock);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
