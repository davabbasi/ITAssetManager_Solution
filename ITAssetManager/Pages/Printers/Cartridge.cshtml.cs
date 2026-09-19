using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Printers
{
    public class CartridgeModel : PageModel
    {

        private readonly ApplicationDbContext _context;

        public CartridgeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Asset Printer { get; set; } = null!;

        public List<CartridgeConsumption> Consumptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // پیدا کردن پرینتر
            Printer = await _context.Assets
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (Printer == null)
                return NotFound();

            // سابقه مصرف کارتریج این پرینتر
            Consumptions = await _context.CartridgeConsumptions
                .Include(x => x.Product)
                .Where(x => x.PrinterId == id)
                .OrderByDescending(x => x.ConsumptionDate)
                .ToListAsync();

            return Page();
        }

       
    }
}

