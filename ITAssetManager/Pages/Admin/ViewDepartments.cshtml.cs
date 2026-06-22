using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Admin;

[Authorize]
public class ViewDepartmentsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public ViewDepartmentsModel(ApplicationDbContext context) => _context = context;

    public List<VwDepartment> Departments { get; set; } = new();

    public async Task OnGetAsync()
    {
        Departments = await _context.VwDepartments.OrderBy(d => d.Name).ToListAsync();
    }
}