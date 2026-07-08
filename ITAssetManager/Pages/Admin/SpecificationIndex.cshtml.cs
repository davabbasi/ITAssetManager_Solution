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
        [BindProperty]
        public List<Specification>? SpecificationList { get; set; }
        public int SelectedSpecId { get; set; }
        public async Task OnGet()
        {
            SpecificationList= await _context.Specifications.ToListAsync();
        }
    }
}
