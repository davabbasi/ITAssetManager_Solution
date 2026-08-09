using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;

namespace ITAssetManager.Pages.WarehouseManagements.WarehouseTransfers
{
    public class DetailsModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public DetailsModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public WarehouseTransfer WarehouseTransfer { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehousetransfer = await _context.WarehouseTransfers.FirstOrDefaultAsync(m => m.Id == id);
            if (warehousetransfer == null)
            {
                return NotFound();
            }
            else
            {
                WarehouseTransfer = warehousetransfer;
            }
            return Page();
        }
    }
}
