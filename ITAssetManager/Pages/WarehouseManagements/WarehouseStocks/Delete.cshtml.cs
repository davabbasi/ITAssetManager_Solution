using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;

namespace ITAssetManager.Pages.WarehouseManagements.WarehouseStocks
{
    public class DeleteModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public DeleteModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WarehouseStock WarehouseStock { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehousestock = await _context.WarehouseStocks.FirstOrDefaultAsync(m => m.Id == id);

            if (warehousestock == null)
            {
                return NotFound();
            }
            else
            {
                WarehouseStock = warehousestock;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehousestock = await _context.WarehouseStocks.FindAsync(id);
            if (warehousestock != null)
            {
                WarehouseStock = warehousestock;
                _context.WarehouseStocks.Remove(WarehouseStock);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
