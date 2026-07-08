using ITAssetManager.Data;
using ITAssetManager.Models;
using ITAssetManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Admin;

[Authorize(Policy = "RequireAdminRole")]
public class CategoriesModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public CategoriesModel(ApplicationDbContext context) => _context = context;

    public List<Category> Categories { get; set; } = new();

    

    [BindProperty(SupportsGet = true)]
    public CategoryEditViewModel Input { get; set; } =new();
   

    public async Task OnGetAsync()
    {
        Categories = await _context.Categories
            .Include(c => c.Assets)
            .OrderBy(c => c.Name)
            .ToListAsync();


        if (Input.Id>0)
        {
            var cat = await _context.Categories.FindAsync(Input.Id);
            if (cat != null)
            {
                Input.Name = cat.Name;
                Input.Description = cat.Description;
                Input.Type = (int)cat.Type;

            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (Input.Id>0)
        {
            var cat = await _context.Categories.FindAsync(Input.Id);
            if (cat != null) 
            { 
                cat.Name = Input.Name; 
                cat.Description = Input.Description;
                cat.Type = (AssetCategoryType)Input.Type;
            }
            else
            {
                return NotFound();

            }
        }
        else
        {
            _context.Categories.Add(new Category
            {
                Name = Input.Name,
                Description = Input.Description,
                Type = (AssetCategoryType)Input.Type,
            });

        }
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }
}
