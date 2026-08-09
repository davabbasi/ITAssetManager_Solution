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

        public async Task OnGetAsync()
        {
            WarehouseStock = await _context.WarehouseStocks
                .Include(w => w.Product)
                .Include(w => w.Warehouse).ToListAsync();
        }
    }
}
