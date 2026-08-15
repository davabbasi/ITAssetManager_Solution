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

namespace ITAssetManager.Pages.WarehouseManagements.Issues
{
    public class EditModel : PageModel
    {
        private readonly ITAssetManager.Data.ApplicationDbContext _context;

        public EditModel(ITAssetManager.Data.ApplicationDbContext context)
        {
            _context = context;
        }
        [BindProperty] public WarehouseIssue WarehouseIssue { get; set; } = new();

        [BindProperty] public List<WarehouseIssueItem> Items { get; set; } = new();

        public SelectList WarehouseList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

        public SelectList KeeperList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

        public List<Product> Products { get; set; } = new();

        [BindProperty] public string ShamsiDate { get; set; }
        public SelectList EmployeeList { get; set; } = null!;


        // =========================
        // GET
        // =========================

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var issue = await _context.WarehouseIssues
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (issue == null)
                return NotFound();

            WarehouseIssue = issue;
            ShamsiDate = WarehouseIssue.IssueDate.ToShamsi();
            Items = issue.Items
                .OrderBy(x => x.RowNumber)
                .ToList();

            await LoadListsAsync();
            return Page();
        }


        // =========================
        // POST
        // =========================

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadListsAsync();
                return Page();
            }

            var issue = await _context.WarehouseIssues
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == WarehouseIssue.Id);

            if (issue == null)
                return NotFound();


            // =========================
            // ویرایش هدر
            // =========================

            issue.IssueNumber = WarehouseIssue.IssueNumber;
            issue.WarehouseId = WarehouseIssue.WarehouseId;
            issue.Description = WarehouseIssue.Description;
            issue.IssueDate = (DateTime)ShamsiDate.ToMiladi();
            // =========================
            // حذف اقلام قبلی
            // =========================

            _context.WarehouseIssueItems.RemoveRange(issue.Items);


            // =========================
            // ثبت اقلام جدید
            // =========================

            if (Items != null)
            {
                int rowNumber = 1;

                foreach (var item in Items)
                {
                    if (item.ProductId <= 0 || item.Quantity <= 0)
                        continue;

                    _context.WarehouseIssueItems.Add(
                        new WarehouseIssueItem
                        {
                            IssueId = issue.Id,
                            RowNumber = rowNumber++,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity
                        });
                }
            }


            await _context.SaveChangesAsync();

            TempData["Success"] = "حواله انبار با موفقیت ویرایش شد.";

            return RedirectToPage(
                "/WarehouseManagements/Issues/Details",
                new { id = issue.Id });
        }


        // =========================
        // لیست‌های فرم
        // =========================

        private async Task LoadListsAsync()
        {
            var warehouses = await _context.Warehouses
                .OrderBy(x => x.WarehouseName)
                .ToListAsync();

            WarehouseList = new SelectList(
                warehouses,
                "Id",
                "WarehouseName",
                WarehouseIssue.WarehouseId
            );

            EmployeeList = new SelectList(
                await _context.VwEmployees
              .OrderBy(e => e.FullName)
              .Select(e => new { e.Id, Name = e.FullName + " - " + e.DepartmentName })
              .ToListAsync(), "Id", "Name",
              WarehouseIssue.EmployeeId
              );

           
            Products = await _context.Products
                .OrderBy(x => x.ProductName)
                .ToListAsync();

        }
    }
}
