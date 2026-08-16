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
    public class IndexModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public IndexModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<WarehouseKeeper> WarehouseKeeper { get;set; } = default!;

        public async Task OnGetAsync()
        {
            WarehouseKeeper = await _context.WarehouseKeepers.ToListAsync();
        }
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var keeperWarehouses = await _context.Warehouses
                .Include(s => s.Keeper)
                .FirstOrDefaultAsync(s => s.KeeperId == id);

            if (keeperWarehouses != null)
            {
                TempData["KeeperDeleteError"] = "برای این این انباردار انبار تعریف شده است  .";
                return RedirectToPage();
            }
            var keeper = await _context.WarehouseKeepers.Where(p => p.Id == id).FirstOrDefaultAsync();
            if (keeper == null)
                return NotFound();
            _context.WarehouseKeepers.Remove(keeper);
            await _context.SaveChangesAsync();
            TempData["KeeperDeleteSuccess"] = "انباردار با موفقیت حذف شد.";
            return RedirectToPage();
        }
    }
}
