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
    public class IndexModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public IndexModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<WarehouseTransfer> WarehouseTransfer { get;set; } = default!;
        [BindProperty(SupportsGet = true)] public int? WarehouseId { get; set; }
        [BindProperty(SupportsGet = true)] public int? TransferNumber { get; set; }
        public List<Warehouse> WarehouseList { get; set; } = null!;
        [BindProperty(SupportsGet = true)] public int? Status { get; set; }

        public async Task OnGetAsync()
        {
            WarehouseList = await _context.Warehouses.Include(r => r.Keeper).ToListAsync();

            WarehouseTransfer = await _context.WarehouseTransfers
                .Include(w => w.DestinationWarehouse)
                .Include(w => w.SourceWarehouse).ToListAsync();
        }
    }
}
