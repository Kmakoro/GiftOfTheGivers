using GiftOfTheGivers.Models;
using GiftOfTheGivers.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAuditService _audit;

    public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IAuditService audit)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _audit = audit;
    }

    [BindProperty] public RegisterInput Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            EmailConfirmed = true,
            FirstName = Input.FirstName.Trim(),
            LastName = Input.LastName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Donor");
            await _audit.WriteAsync("DonorRegistered", user.Id, nameof(ApplicationUser), user.Id, $"Email={user.Email}");
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["AccountMessage"] = "Registration successful. Your Donor account is ready.";
            return RedirectToPage("/Donor/Dashboard");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
        return Page();
    }

    public class RegisterInput
    {
        [Required, Display(Name = "First name"), StringLength(80)] public string FirstName { get; set; } = string.Empty;
        [Required, Display(Name = "Last name"), StringLength(80)] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
        [Required, DataType(DataType.Password), MinLength(8)] public string Password { get; set; } = string.Empty;
        [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Confirm password")] public string ConfirmPassword { get; set; } = string.Empty;
    }
}
