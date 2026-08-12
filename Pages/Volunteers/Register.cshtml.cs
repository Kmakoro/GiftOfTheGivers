using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages.Volunteers;

public class RegisterModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty]
    public VolunteerInput Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var exists = await _db.Volunteers.AnyAsync(v => v.Email == Input.Email);
        if (exists)
        {
            ModelState.AddModelError("Input.Email", "This email is already registered as a volunteer.");
            return Page();
        }

        _db.Volunteers.Add(new Volunteer
        {
            FullName = Input.FullName,
            Email = Input.Email,
            PhoneNumber = Input.PhoneNumber,
            Skills = Input.Skills,
            Availability = Input.Availability,
            City = Input.City,
            RegisteredAt = DateTime.UtcNow,
            UserId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Thank you {Input.FullName}. Your volunteer registration has been received — a coordinator will be in touch.";
        return RedirectToPage();
    }

    public class VolunteerInput
    {
        [Required, MaxLength(120)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Phone, MaxLength(30)]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [Required, MaxLength(300)]
        [Display(Name = "Skills")]
        public string Skills { get; set; } = string.Empty;

        [Required, MaxLength(120)]
        public string Availability { get; set; } = "Weekends";

        [MaxLength(120)]
        [Display(Name = "City / town")]
        public string? City { get; set; }
    }
}
