using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;

namespace ITAssetManager.Pages.WarehouseManagements.Receipts
{
    public class DeleteModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public DeleteModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WarehouseReceipt WarehouseReceipt { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehousereceipt = await _context.WarehouseReceipts.FirstOrDefaultAsync(m => m.Id == id);

            if (warehousereceipt == null)
            {
                return NotFound();
            }
            else
            {
                WarehouseReceipt = warehousereceipt;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehousereceipt = await _context.WarehouseReceipts.FindAsync(id);
            if (warehousereceipt != null)
            {
                WarehouseReceipt = warehousereceipt;
                _context.WarehouseReceipts.Remove(WarehouseReceipt);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
