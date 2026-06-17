using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Admin;

[Authorize(Policy = "RequireAdminRole")]
public class EmployeesModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public EmployeesModel(ApplicationDbContext context) => _context = context;

    public List<Employee> Employees { get; set; } = new();
    public List<Department> Departments { get; set; } = new();

    [BindProperty(SupportsGet = true)] public int? EditId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? DeptFilter { get; set; }

    public string? EditFullName { get; set; }
    public string? EditEmployeeCode { get; set; }
    public string? EditPosition { get; set; }
    public int? EditDepartmentId { get; set; }

    public async Task OnGetAsync()
    {
        Departments = await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync();

        var query = _context.Employees.Include(e => e.Department).AsQueryable();
        if (!string.IsNullOrEmpty(Search))
            query = query.Where(e => e.FullName.Contains(Search) ||
                (e.EmployeeCode != null && e.EmployeeCode.Contains(Search)));
        if (DeptFilter.HasValue)
            query = query.Where(e => e.DepartmentId == DeptFilter);

        Employees = await query.OrderBy(e => e.FullName).ToListAsync();

        if (EditId.HasValue)
        {
            var emp = await _context.Employees.FindAsync(EditId);
            if (emp != null)
            {
                EditFullName = emp.FullName;
                EditEmployeeCode = emp.EmployeeCode;
                EditPosition = emp.Position;
                EditDepartmentId = emp.DepartmentId;
            }
        }
    }

    public async Task<IActionResult> OnPostAsync(int? editId, string fullName, string? employeeCode,
        string? position, int departmentId)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return RedirectToPage();

        if (editId.HasValue)
        {
            var emp = await _context.Employees.FindAsync(editId);
            if (emp != null)
            {
                emp.FullName = fullName;
                emp.EmployeeCode = employeeCode;
                emp.Position = position;
                emp.DepartmentId = departmentId;
            }
        }
        else
        {
            _context.Employees.Add(new Employee
            {
                FullName = fullName,
                EmployeeCode = employeeCode,
                Position = position,
                DepartmentId = departmentId
            });
        }
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id)
    {
        var emp = await _context.Employees.FindAsync(id);
        if (emp != null) { emp.IsActive = !emp.IsActive; await _context.SaveChangesAsync(); }
        return RedirectToPage();
    }
}
