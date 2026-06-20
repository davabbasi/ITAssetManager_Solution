using ITAssetManager.Data;
using ITAssetManager.Models;
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
    [BindProperty(SupportsGet = true)] public int? EditId { get; set; }

   
    public int EditType { get; set; } = 1;
    public string? EditName { get; set; }
    public string? EditIcon { get; set; }
    public string? EditDescription { get; set; }

    public async Task OnGetAsync()
    {
        Categories = await _context.AssetCategories
            .Include(c => c.Assets)
            .OrderBy(c => c.Name)
            .ToListAsync();


        if (EditId.HasValue)
        {
            var cat = await _context.AssetCategories.FindAsync(EditId);
            if (cat != null)
            {
                EditName = cat.Name;
                EditIcon = cat.Icon;
                EditDescription = cat.Description;
                EditType = (int)cat.Type;

            }
        }
    }

    public async Task<IActionResult> OnPostAsync(int? editId, string name, string? icon, string? description, int type = 1)
    {
        if (string.IsNullOrWhiteSpace(name)) return RedirectToPage();

        if (editId.HasValue)
        {
            var cat = await _context.AssetCategories.FindAsync(editId);
            if (cat != null) 
            { 
                cat.Name = name; 
                cat.Icon = icon ?? "bi-box";
                cat.Description = description;
                cat.Type = (AssetCategoryType)type;
            }
        }
        else
        {
            _context.AssetCategories.Add(new AssetCategory
            {
                Name = name,
                Icon = icon ?? "bi-box",
                Description = description,
                Type = (AssetCategoryType)type
            });

        }
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }
}
