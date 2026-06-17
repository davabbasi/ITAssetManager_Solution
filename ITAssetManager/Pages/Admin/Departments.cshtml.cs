using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Admin;

[Authorize(Policy = "RequireAdminRole")]
public class DepartmentsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public DepartmentsModel(ApplicationDbContext context) => _context = context;

    public List<Department> Departments { get; set; } = new();
    [BindProperty(SupportsGet = true)] public int? EditId { get; set; }
    public string? EditName { get; set; }
    public string? EditDescription { get; set; }

    public async Task OnGetAsync()
    {
        Departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
        if (EditId.HasValue)
        {
            var dept = await _context.Departments.FindAsync(EditId);
            if (dept != null) { EditName = dept.Name; EditDescription = dept.Description; }
        }
    }

    public async Task<IActionResult> OnPostAsync(int? editId, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) return RedirectToPage();

        if (editId.HasValue)
        {
            var dept = await _context.Departments.FindAsync(editId);
            if (dept != null) { dept.Name = name; dept.Description = description; }
        }
        else
        {
            _context.Departments.Add(new Department { Name = name, Description = description });
        }
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }
}
