using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;

namespace ITAssetManager.Pages.WarehouseManagements.Issues
{
    public class IndexModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public IndexModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }
        [BindProperty(SupportsGet = true)] public int? IssueNumber { get; set; }
        [BindProperty(SupportsGet = true)] public int? Status { get; set; }
        [BindProperty(SupportsGet = true)] public int? WarehouseId { get; set; }
        public List<Warehouse> WarehouseList { get; set; } = null!;
        public IList<WarehouseIssue> WarehouseIssue { get;set; } = default!;
        public int TotalCount { get; set; }


        public async Task OnGetAsync()
        {
            WarehouseList = await _context.Warehouses.Include(r => r.Keeper).ToListAsync();

            var query = _context.WarehouseIssues.Include(r => r.Warehouse).AsQueryable();
            if (IssueNumber.HasValue)
                query = query.Where(r => r.IssueNumber == IssueNumber);
            if (WarehouseId.HasValue)
                query = query.Where(r => r.WarehouseId == WarehouseId);
            if (Status.HasValue)
                query = query.Where(a => (int)a.Status == Status);
           
            TotalCount = await query.CountAsync();

            WarehouseIssue = await query.OrderBy(a => a.IssueNumber).ToListAsync();
        }
    }
}
