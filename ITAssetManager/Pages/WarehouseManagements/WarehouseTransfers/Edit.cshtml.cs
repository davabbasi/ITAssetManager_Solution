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

namespace ITAssetManager.Pages.WarehouseManagements.WarehouseTransfers
{
    public class EditModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public EditModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WarehouseTransfer WarehouseTransfer { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehousetransfer =  await _context.WarehouseTransfers.FirstOrDefaultAsync(m => m.Id == id);
            if (warehousetransfer == null)
            {
                return NotFound();
            }
            WarehouseTransfer = warehousetransfer;
           ViewData["DestinationWarehouseId"] = new SelectList(_context.Warehouses, "Id", "WarehouseName");
           ViewData["SourceWarehouseId"] = new SelectList(_context.Warehouses, "Id", "WarehouseName");
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

            _context.Attach(WarehouseTransfer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WarehouseTransferExists(WarehouseTransfer.Id))
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

        private bool WarehouseTransferExists(int id)
        {
            return _context.WarehouseTransfers.Any(e => e.Id == id);
        }
    }
}
