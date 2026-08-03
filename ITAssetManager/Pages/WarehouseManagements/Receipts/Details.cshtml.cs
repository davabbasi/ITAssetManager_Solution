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
    public class DetailsModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public DetailsModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public WarehouseReceipt WarehouseReceipt { get; set; } = default!;
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WarehouseReceipt = await _context.WarehouseReceipts
             .Include(x => x.Warehouse)
             .Include(x => x.Items)
             .ThenInclude(x => x.Product)
             .FirstOrDefaultAsync(x => x.Id == id);

            if (WarehouseReceipt == null)
            {
                return NotFound();
            }
           
            return Page();
        }
    }
}
