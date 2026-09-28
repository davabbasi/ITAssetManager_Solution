using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Warehouses
{
    [Authorize(Policy = "RequireITRole")]

    public class EditModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public EditModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Warehouse Warehouse { get; set; } = default!;
        public SelectList KeeperList { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(x => x.Id == id);
            if (Warehouse == null)
                return NotFound();
            await LoadLists();
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadLists();
                return Page();
            }

            _context.Attach(Warehouse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WarehouseExists(Warehouse.Id))
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

        private bool WarehouseExists(int id)
        {
            return _context.Warehouses.Any(e => e.Id == id);
        }
        private async Task LoadLists()
        {
            var keepers = await _context.WarehouseKeepers
                .OrderBy(x => x.FullName)
                .Select(x => new
                {
                    x.Id,
                    Name = x.FullName
                })
                .ToListAsync();

            KeeperList = new SelectList(
                keepers,
                "Id",
                "Name",
                Warehouse.KeeperId
            );


        }
    }
}
