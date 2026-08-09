using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.WarehouseManagements.Issues
{
    public class CreateModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;
      
        public CreateModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }
        public SelectList WarehouseList { get; set; } = null!;
        public List<Product> Products { get; set; } = new();
        [BindProperty] public string ShamsiDate { get; set; }
        public async Task< IActionResult> OnGet()
        {
            await LoadLists();
            return Page();
        }

        [BindProperty]
        public WarehouseIssue WarehouseIssue { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadLists();
                return Page();
            }

            _context.WarehouseIssues.Add(WarehouseIssue);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task LoadLists()
        {
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
    }
}
