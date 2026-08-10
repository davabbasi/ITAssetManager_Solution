using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.WarehouseManagements.Receipts
{
    public class IndexModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public IndexModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }
        [BindProperty(SupportsGet = true)] public int? Status { get; set; }
        [BindProperty(SupportsGet = true)] public int? WarehouseId { get; set; }
        [BindProperty(SupportsGet = true)] public int? ReceiptNumber { get; set; }
        [BindProperty(SupportsGet = true)] public string? PurchaseRequest { get; set; }
        public int TotalCount { get; set; }


        public List<Warehouse> WarehouseList { get; set; } = null!;
        public IList<WarehouseReceipt> WarehouseReceipt { get;set; } = default!;

        public async Task OnGetAsync()
        {
            WarehouseList = await _context.Warehouses.Include(r => r.Keeper).ToListAsync();

            var query = _context.WarehouseReceipts.Include(r=>r.Warehouse).AsQueryable();
            if (ReceiptNumber.HasValue)
                query = query.Where(r => r.ReceiptNumber == ReceiptNumber);
            if (WarehouseId.HasValue)
                query = query.Where(r => r.WarehouseId == WarehouseId);
            if (Status.HasValue)
                query = query.Where(a => (int)a.Status == Status);
            if (!string.IsNullOrEmpty(PurchaseRequest))
                query = query.Where(a => a.ReferenceNumber.Contains(PurchaseRequest) );
            TotalCount = await query.CountAsync();
            WarehouseReceipt = await query.OrderBy(a => a.ReceiptNumber).ToListAsync();

        }
    }
}
