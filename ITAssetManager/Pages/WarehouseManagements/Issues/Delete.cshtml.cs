using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.WarehouseManagements.Issues
{
    [Authorize(Policy = "RequireITRole")]

    public class DeleteModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public DeleteModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WarehouseIssue WarehouseIssue { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehouseissue = await _context.WarehouseIssues.FirstOrDefaultAsync(m => m.Id == id);

            if (warehouseissue == null)
            {
                return NotFound();
            }
            else
            {
                WarehouseIssue = warehouseissue;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehouseissue = await _context.WarehouseIssues.FindAsync(id);
            if (warehouseissue != null)
            {
                WarehouseIssue = warehouseissue;
                _context.WarehouseIssues.Remove(WarehouseIssue);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
