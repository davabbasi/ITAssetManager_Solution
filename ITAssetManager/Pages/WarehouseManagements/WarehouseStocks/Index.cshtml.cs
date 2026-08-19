using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;

namespace ITAssetManager.Pages.WarehouseManagements.WarehouseStocks
{
    public class IndexModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public IndexModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<WarehouseStock> WarehouseStock { get;set; } = default!;
        public List<Warehouse> WarehouseList { get; set; } = null!;
        [BindProperty(SupportsGet = true)] public string? ProductName { get; set; }
        [BindProperty(SupportsGet = true)] public int? WarehouseId { get; set; }


        public async Task OnGetAsync()
        {
            WarehouseList = await _context.Warehouses.Include(r => r.Keeper).ToListAsync();

            var query =  _context.WarehouseStocks
                .Include(w => w.Product)
                .Include(w => w.Warehouse).AsQueryable();
            if(!string.IsNullOrEmpty(ProductName))
                query=query.Where(x=>x.Product.ProductName.Contains(ProductName));
            if(WarehouseId.HasValue)
                query= query.Where(x=>x.WarehouseId==WarehouseId);

            WarehouseStock = await query.OrderBy(w=>w.WarehouseId).ToListAsync();
        }
    }
}
