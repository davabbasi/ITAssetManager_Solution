using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITAssetManager.Convertor;
using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Warehouses
{
    public class CreateModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;
        public SelectList KeeperList { get; set; } = null!;
        public SelectList OwnerList { get; set; } = null!;

        public CreateModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGet()
        {
            await LoadLists();
            return Page();
        }

        [BindProperty] public Warehouse Warehouse { get; set; } = default!;
        public SelectList WarehouseTypeList { get; set; } = null!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadLists();
                return Page();
            }
            var selectedOwner =await  _context.VwDepartments.Where(x => x.Id == Warehouse.WarehouseOwnerID).FirstOrDefaultAsync();
            if (selectedOwner != null)
                Warehouse.WarehouseOwner = selectedOwner.Name;
            _context.Warehouses.Add(Warehouse);
            await _context.SaveChangesAsync();

         

            return RedirectToPage("./Index");
        }
        private async Task LoadLists()
        {

            WarehouseTypeList = ConvertEnumToSelect.ToSelectList<WarehouseType>();
            KeeperList = new SelectList(
                await _context.WarehouseKeepers
                    .OrderBy(x => x.FullName)
                    .Select(x => new
                    {
                        x.Id,
                        Name = x.FullName+"-"+x.PersonnelNumber
                    })
                    .ToListAsync(),
                "Id",
                "Name");
            OwnerList = new SelectList(
                await _context.VwDepartments
                    .OrderBy(x => x.Name)
                    .Select(x => new
                    {
                        x.Id,
                        Name = x.Name
                    })
                    .ToListAsync(),
                "Id",
                "Name");

        }
    }
}
