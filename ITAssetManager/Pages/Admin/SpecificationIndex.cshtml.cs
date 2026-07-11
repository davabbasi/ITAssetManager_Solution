using System.Threading.Tasks;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Admin
{
    public class SpecificationIndex : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SpecificationIndex(ApplicationDbContext context)
        {
            _context=context;
        }
        public List<Specification>? SpecificationList { get; set; }
        public int SelectedSpecId { get; set; }
        public async Task OnGet()
        {
            SpecificationList= await _context.Specifications.Include(s => s.SpecValues).ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var spec = await _context.Specifications
                .Include(s => s.SpecValues)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (spec == null)
                return NotFound();

            if (spec.SpecValues.Any())
            {
                TempData["Error"] = "ابتدا تمام مقادیر این مشخصه را حذف کنید.";
                return RedirectToPage();
            }

            _context.Specifications.Remove(spec);
            await _context.SaveChangesAsync();

            TempData["Success"] = "مشخصه با موفقیت حذف شد.";

            return RedirectToPage();
        }
    }


}
