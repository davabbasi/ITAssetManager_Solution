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
    public class IndexModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public IndexModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<InventoryTransaction> InventoryTransaction { get;set; } = default!;
        public List<Warehouse> WarehouseList { get; set; } = null!;
        [BindProperty(SupportsGet = true)] public int? WarehouseId { get; set; }
        [BindProperty(SupportsGet = true)] public int? Type { get; set; } 

        public async Task OnGetAsync()
        {
            WarehouseList = await _context.Warehouses.Include(r => r.Keeper).ToListAsync();
            var query =  _context.InventoryTransactions
                .Include(i => i.Product)
                .Include(i => i.Warehouse).AsQueryable();
            if (Type.HasValue)
                query = query.Where(x => (int)x.Type == Type);
            if(WarehouseId.HasValue)
                query = query.Where(x => x.WarehouseId == WarehouseId);

            InventoryTransaction =await query.ToListAsync();
        }
    }
}
