using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;

namespace ITAssetManager.Pages.Warehouses
{
    public class IndexModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public IndexModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Warehouse> Warehouse { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Warehouse = await _context.Warehouses.Include(w=>w.Keeper).ToListAsync();
        }
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var warehousesTransactions = await _context.InventoryTransactions
                
                .FirstOrDefaultAsync(s => s.WarehouseId == id);
            if (warehousesTransactions != null)
            {
                TempData["WarehouseDeleteError"] = "برای این این انبار تراکنش انبار ثبت شده است   .";
                return RedirectToPage();
            }
            var warehouse = await _context.Warehouses.Where(p => p.Id == id).FirstOrDefaultAsync();
            if (warehouse == null)
                return NotFound();
            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();
            TempData["WarehouseDeleteSuccess"] = "انبار با موفقیت حذف شد.";
            return RedirectToPage();
        }
    }
}
