using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;
using ITAssetManager.Convertor;

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
        [BindProperty] public string ShamsiDate { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WarehouseTransfer = await _context.WarehouseTransfers
                .Include(t=>t.Items)
                .ThenInclude(t => t.Product)
                .Include(t => t.SourceWarehouse)
                .Include(t=>t.DestinationWarehouse)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (WarehouseTransfer == null)
            {
                return NotFound();
            }
           

            ShamsiDate = WarehouseTransfer.TransferDate.ToShamsi();

          
            return Page();
        }
    }
}
