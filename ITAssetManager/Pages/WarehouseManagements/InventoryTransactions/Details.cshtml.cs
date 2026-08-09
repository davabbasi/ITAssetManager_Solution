using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;

namespace ITAssetManager.Pages.WarehouseManagements.InventoryTransactions
{
    public class DetailsModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public DetailsModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public InventoryTransaction InventoryTransaction { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventorytransaction = await _context.InventoryTransactions.FirstOrDefaultAsync(m => m.Id == id);
            if (inventorytransaction == null)
            {
                return NotFound();
            }
            else
            {
                InventoryTransaction = inventorytransaction;
            }
            return Page();
        }
    }
}
