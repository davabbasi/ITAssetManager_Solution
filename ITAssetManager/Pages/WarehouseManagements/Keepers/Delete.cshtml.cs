using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;

namespace ITAssetManager.Pages.WarehouseManagements.Keeper
{
    public class DeleteModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public DeleteModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehousekeeper = await _context.WarehouseKeepers.FindAsync(id);
            if (warehousekeeper != null)
            {
                WarehouseKeeper = warehousekeeper;
                _context.WarehouseKeepers.Remove(WarehouseKeeper);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
