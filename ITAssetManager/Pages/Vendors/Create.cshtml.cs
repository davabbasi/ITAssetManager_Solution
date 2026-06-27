using ITAssetManager.Data;
using ITAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Vendors;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public CreateModel(ApplicationDbContext context) => _context = context;

    [BindProperty] public Vendor Vendor { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Vendor.Assets");
        ModelState.Remove("Vendor.Code");  // ← اضافه کن چون خودمون پرش می‌کنیم
        if (!ModelState.IsValid) return Page();

        // تولید کد اتوماتیک
        var lastId = await _context.Vendors.MaxAsync(v => (int?)v.Id) ?? 0;
        Vendor.Code = $"{(lastId + 1):D4}";  // مثلاً: V-0001، V-0042

        Vendor.CreatedAt = DateTime.Now;
        _context.Vendors.Add(Vendor);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"فروشنده «{Vendor.Name}» با موفقیت ثبت شد.";
        return RedirectToPage("/Vendors/Index");
    }
}