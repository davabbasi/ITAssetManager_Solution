using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManager.Pages.Admin;

[Authorize(Policy = "RequireAdminRole")]
public class UsersModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public List<IdentityUser> Users { get; set; } = new();
    public Dictionary<string, List<string>> UserRoles { get; set; } = new();

    public async Task OnGetAsync()
    {
        Users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        foreach (var user in Users)
            UserRoles[user.Id] = (await _userManager.GetRolesAsync(user)).ToList();
    }

    public async Task<IActionResult> OnPostCreateUserAsync(string email, string password, string role)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            TempData["Error"] = "ایمیل و رمز عبور الزامی است.";
            return RedirectToPage();
        }

        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role ?? "User");
            TempData["Success"] = $"کاربر {email} با موفقیت ایجاد شد.";
        }
        else
        {
            TempData["Error"] = string.Join(" | ", result.Errors.Select(e => e.Description));
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleRoleAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return RedirectToPage();

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
            await _userManager.AddToRoleAsync(user, "User");
            TempData["Success"] = "نقش کاربر به User تغییر یافت.";
        }
        else
        {
            await _userManager.RemoveFromRoleAsync(user, "User");
            await _userManager.AddToRoleAsync(user, "Admin");
            TempData["Success"] = "نقش کاربر به Admin تغییر یافت.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
            TempData["Success"] = "کاربر حذف شد.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync(string userId, string newPassword)
    {
        if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
        {
            TempData["Error"] = "رمز عبور باید حداقل ۶ کاراکتر باشد.";
            return RedirectToPage();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "رمز عبور با موفقیت تغییر یافت."
                : string.Join(" | ", result.Errors.Select(e => e.Description));
        }
        return RedirectToPage();
    }
}
