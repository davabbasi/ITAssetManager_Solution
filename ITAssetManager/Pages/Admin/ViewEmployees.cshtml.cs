using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Admin;

[Authorize]
public class ViewEmployeesModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public ViewEmployeesModel(ApplicationDbContext context) => _context = context;

    public List<VwEmployee> Employees { get; set; } = new();

    public async Task OnGetAsync()
    {
        Employees = await _context.VwEmployees
            .OrderBy(e => e.DepartmentName)
            .ThenBy(e => e.FullName)
            .ToListAsync();
    }
}