using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages.Donations;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IReferenceGenerator _references;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(ApplicationDbContext db, IReferenceGenerator references, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _references = references;
        _userManager = userManager;
    }

    [BindProperty]
    public DonationInput Input { get; set; } = new();

    public SelectList ProjectOptions { get; private set; } = new(Enumerable.Empty<ReliefProject>());

    public async Task OnGetAsync(int? projectId)
    {
        await LoadProjectsAsync();
        Input.ReliefProjectId = projectId;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                Input.DonorName = user.FullName;
                Input.DonorEmail = user.Email;
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.IsAnonymous)
        {
            // Anonymous guests do not need to identify themselves.
            ModelState.Remove($"{nameof(Input)}.{nameof(Input.DonorName)}");
            ModelState.Remove($"{nameof(Input)}.{nameof(Input.DonorEmail)}");
        }

        if (!ModelState.IsValid)
        {
            await LoadProjectsAsync();
            return Page();
        }

        var donation = new Donation
        {
            Reference = _references.NewDonationReference(),
            Amount = Input.Amount,
            Currency = Input.Currency,
            Frequency = Input.Frequency,
            IsAnonymous = Input.IsAnonymous,
            DonorName = Input.IsAnonymous ? "Anonymous" : Input.DonorName!,
            DonorEmail = Input.IsAnonymous ? null : Input.DonorEmail,
            ReliefProjectId = Input.ReliefProjectId,
            DonatedAt = DateTime.UtcNow,
            UserId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null
        };

        donation.TaxCertificate = new TaxCertificate
        {
            CertificateNumber = _references.NewCertificateNumber(),
            IssuedAt = DateTime.UtcNow
        };

        _db.Donations.Add(donation);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Donations/ThankYou", new { reference = donation.Reference });
    }

    private async Task LoadProjectsAsync()
    {
        var projects = await _db.ReliefProjects
            .AsNoTracking()
            .Where(p => p.Status != ProjectStatus.Completed)
            .OrderBy(p => p.Name)
            .ToListAsync();

        ProjectOptions = new SelectList(projects, nameof(ReliefProject.Id), nameof(ReliefProject.Name));
    }

    public class DonationInput
    {
        [Range(1, 10_000_000, ErrorMessage = "Enter an amount of at least 1.")]
        public decimal Amount { get; set; } = 250m;

        public Currency Currency { get; set; } = Currency.ZAR;

        [Display(Name = "Donation type")]
        public DonationFrequency Frequency { get; set; } = DonationFrequency.OneTime;

        [Display(Name = "Donate anonymously")]
        public bool IsAnonymous { get; set; }

        [Display(Name = "Your name")]
        [MaxLength(120)]
        [RequiredIfNotAnonymous]
        public string? DonorName { get; set; }

        [Display(Name = "Email address")]
        [EmailAddress]
        [MaxLength(200)]
        public string? DonorEmail { get; set; }

        [Display(Name = "Relief project")]
        public int? ReliefProjectId { get; set; }
    }

    /// <summary>Requires a value unless the donor chose to remain anonymous.</summary>
    public class RequiredIfNotAnonymousAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (context.ObjectInstance is DonationInput input && input.IsAnonymous)
                return ValidationResult.Success;

            return value is string s && !string.IsNullOrWhiteSpace(s)
                ? ValidationResult.Success
                : new ValidationResult("Please tell us your name, or choose to donate anonymously.");
        }
    }
}
