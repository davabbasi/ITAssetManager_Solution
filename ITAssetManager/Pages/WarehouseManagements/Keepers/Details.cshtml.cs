using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.WarehouseManagements.Keeper
{
    [Authorize(Policy = "RequireITRole")]

    public class DetailsModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public DetailsModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public WarehouseKeeper WarehouseKeeper { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehousekeeper = await _context.WarehouseKeepers.FirstOrDefaultAsync(m => m.Id == id);
            if (warehousekeeper == null)
            {
                return NotFound();
            }
            else
            {
                WarehouseKeeper = warehousekeeper;
            }
            return Page();
        }
    }
}
