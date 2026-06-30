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

    public List<AssetCategory> Categories { get; set; } = new();

    

    [BindProperty(SupportsGet = true)]
    public CategoryEditViewModel Input { get; set; } =new();
   

    public async Task OnGetAsync()
    {
        Categories = await _context.AssetCategories
            .Include(c => c.Assets)
            .OrderBy(c => c.Name)
            .ToListAsync();


        if (Input.Id>0)
        {
            var cat = await _context.AssetCategories.FindAsync(Input.Id);
            if (cat != null)
            {
                Input.Name = cat.Name;
                Input.Icon = cat.Icon;
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
            var cat = await _context.AssetCategories.FindAsync(Input.Id);
            if (cat != null) 
            { 
                cat.Name = Input.Name; 
                cat.Icon = Input.Icon ?? "bi-box";
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
            _context.AssetCategories.Add(new AssetCategory
            {
                Name = Input.Name,
                Icon = Input.Icon ?? "bi-box",
                Description = Input.Description,
                Type = (AssetCategoryType)Input.Type,
            });

        }
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }
}
