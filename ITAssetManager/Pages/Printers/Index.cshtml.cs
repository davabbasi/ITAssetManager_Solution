using ITAssetManager.Convertor;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Printers
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public List<Asset> Assets { get; set; } = new();
        public List<VwDepartment> Departments { get; set; } = new();
        public List<VwEmployee> Employees { get; set; } = new();
        public int TotalCount { get; set; }
        public Dictionary<int, string> ComponentLocations { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
        [BindProperty(SupportsGet = true)] public int? Status { get; set; }
        [BindProperty(SupportsGet = true)] public int? DepartmentId { get; set; }
        [BindProperty(SupportsGet = true)] public int? EmployeeId { get; set; }
        public SelectList StatusSelect { get; set; } = null!;
        public async Task OnGet()
        {
            StatusSelect = ConvertEnumToSelect.ToSelectList<AssetStatus>();
            Departments = await _context.VwDepartments.OrderBy(d => d.Name).ToListAsync();
            Employees = await _context.VwEmployees.OrderBy(e => e.FullName).ToListAsync();

            VwDepartment vwDepartment = new()
            {
                Id = 1,
                Name = "انفورماتیک"
            };
            Departments.Add(vwDepartment);

            var query = BuildQuery();

           
            Assets = await query.Where(x=>x.CategoryId==4).OrderBy(a => a.Id).ToListAsync();
            TotalCount = Assets.Count();
            //var assemblyComponents = await _context.AssemblyComponents
            //    .Include(x => x.PcAsset)
            //    .Where(x => x.RemovedAt == null)
            //    .ToListAsync();

            //ComponentLocations = assemblyComponents
            //    .Where(x => x.PcAsset != null)
            //    .ToDictionary(
            //    x => x.ComponentAssetId,
            //    x => x.PcAsset.EmployeeName != null ? $"{x.PcAsset.Name} - {x.PcAsset.EmployeeName}"
            //    : $"{x.PcAsset.Name} - {x.PcAsset.DepartmentName}"
            //    );
        }

        private IQueryable<Asset> BuildQuery()
        {
            var query = _context.Assets
                .Include(a => a.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(Search))
            {
                query = query.Where(a =>
                    a.Name.Contains(Search) ||
                    (a.SerialNumber != null && a.SerialNumber.Contains(Search)) ||
                    (a.Barcode != null && a.Barcode.Contains(Search)) ||
                    (a.PropertyTag != null && a.PropertyTag.Contains(Search)) ||
                    (a.Model != null && a.Model.Contains(Search)));
            }

         

            if (Status.HasValue)
                query = query.Where(a => (int)a.Status == Status);

            if (DepartmentId.HasValue)
                query = query.Where(a => a.DepartmentId == DepartmentId);

            if (EmployeeId.HasValue)
                query = query.Where(a => a.EmployeeId == EmployeeId);

            return query.OrderBy(a => a.Id);
        }

    }
}
