using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITAssetManager.Data;
using ITAssetManager.Models;

namespace ITAssetManager.Pages.WarehouseManagements.InventoryTransactions
{
    public class EditModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public EditModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InventoryTransaction InventoryTransaction { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventorytransaction =  await _context.InventoryTransactions.FirstOrDefaultAsync(m => m.Id == id);
            if (inventorytransaction == null)
            {
                return NotFound();
            }
            InventoryTransaction = inventorytransaction;
           ViewData["ProductId"] = new SelectList(_context.Products, "Id", "ProductName");
           ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "WarehouseName");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(InventoryTransaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryTransactionExists(InventoryTransaction.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool InventoryTransactionExists(long id)
        {
            return _context.InventoryTransactions.Any(e => e.Id == id);
        }
    }
}
