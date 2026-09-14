using GiftOfTheGivers.Models;
using GiftOfTheGivers.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _audit;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IAuditService audit)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _audit = audit;
    }

    [BindProperty] public LoginInput Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Page();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is not null && await _userManager.IsInRoleAsync(currentUser, "Employee"))
            return RedirectToPage("/Employee/Dashboard");
        if (currentUser is not null && await _userManager.IsInRoleAsync(currentUser, "Donor"))
            return RedirectToPage("/Donor/Dashboard");

        return RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var email = Input.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            await TryAuditAsync("LoginFailed", details: $"Email={email}");
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            await TryAuditAsync(result.IsLockedOut ? "LoginLockedOut" : "LoginFailed", user.Id, nameof(ApplicationUser), user.Id, $"Email={email}");
            ModelState.AddModelError(string.Empty, result.IsLockedOut
                ? "This account is temporarily locked. Please wait a few minutes and try again."
                : "Invalid email or password.");
            return Page();
        }

        await TryAuditAsync("LoginSucceeded", user.Id, nameof(ApplicationUser), user.Id, $"Email={user.Email}");

        if (await _userManager.IsInRoleAsync(user, "Employee"))
            return RedirectToPage("/Employee/Dashboard");

        if (await _userManager.IsInRoleAsync(user, "Donor"))
            return RedirectToPage("/Donor/Dashboard");

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return RedirectToPage("/Index");
    }

    private async Task TryAuditAsync(string action, string? userId = null, string? entityName = null, string? entityId = null, string? details = null)
    {
        try { await _audit.WriteAsync(action, userId, entityName, entityId, details); }
        catch { /* Authentication should not fail because activity logging is unavailable. */ }
    }

    public class LoginInput
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
        [Display(Name = "Remember me")] public bool RememberMe { get; set; }
    }
}
