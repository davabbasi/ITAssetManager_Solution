using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;

namespace ITAssetManager.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;
        public List<Category> Categories { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
        public int TotalCount { get; set; }

        public IndexModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Product> Product { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            var query = _context.Products
                .Include(a => a.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(Search))
                query = query.Where(a =>
                a.ProductName.Contains(Search) 
                ||(a.ProductDescription != null 
                && a.ProductDescription.Contains(Search)));

            if (CategoryId.HasValue)
                query = query.Where(a => a.CategoryId == CategoryId);

            TotalCount = await query.CountAsync();

            Product = await query.OrderByDescending(a => a.ProductName).ToListAsync();
        }
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var transaction = await _context.InventoryTransactions
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.ProductId == id);
            if (transaction!=null)
            {
                TempData["ProductError"] = "برای این کالا تراکنش انبار ثبت شده است .";
                return RedirectToPage();
            }
            var product = await _context.Products.Where(p => p.Id == id).FirstOrDefaultAsync();
            if (product == null)
                return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["ProductSuccess"] = "کالا با موفقیت حذف شد.";
            return RedirectToPage();
        }
    }
}
