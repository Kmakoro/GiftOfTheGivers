using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Pages;

public class VolunteerModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public VolunteerModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty] public VolunteerInput Input { get; set; } = new();
    [TempData] public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;

        _db.VolunteerApplications.Add(new VolunteerApplication
        {
            UserId = user?.Id,
            FullName = Input.FullName,
            Email = Input.Email,
            Phone = Input.Phone,
            Skills = Input.Skills,
            Availability = Input.Availability,
            DateSubmitted = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        SuccessMessage = "Thank you. Your volunteer interest has been recorded.";
        return RedirectToPage();
    }

    public class VolunteerInput
    {
        [Required, Display(Name = "Full name"), StringLength(160)] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Phone] public string? Phone { get; set; }
        [Required, StringLength(500)] public string Skills { get; set; } = string.Empty;
        [Required] public string Availability { get; set; } = string.Empty;
    }
}
