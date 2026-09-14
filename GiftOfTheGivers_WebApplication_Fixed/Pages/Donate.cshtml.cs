using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Pages;

public class DonateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _audit;

    public DonateModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IAuditService audit)
    {
        _db = db;
        _userManager = userManager;
        _audit = audit;
    }

    [BindProperty]
    public DonationInput Input { get; set; } = new();

    public List<CurrencyReference> Currencies { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadCurrenciesAsync();
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                Input.DonorName = $"{user.FirstName} {user.LastName}".Trim();
                Input.DonorEmail = user.Email ?? string.Empty;
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCurrenciesAsync();
        var supportedCurrencies = Currencies.Select(c => c.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!supportedCurrencies.Contains(Input.Currency))
            ModelState.AddModelError("Input.Currency", "Please select a supported currency.");
        if (!new[] { "One-time", "Recurring" }.Contains(Input.Frequency))
            ModelState.AddModelError("Input.Frequency", "Please select a valid donation frequency.");

        if (!ModelState.IsValid) return Page();

        var user = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
        var donation = new Donation
        {
            UserId = user?.Id,
            DonorName = Input.DonorName.Trim(),
            DonorEmail = Input.DonorEmail.Trim(),
            Amount = Input.Amount,
            Currency = Input.Currency.ToUpperInvariant(),
            Frequency = Input.Frequency,
            IsAnonymous = Input.IsAnonymous,
            DonationDate = DateTime.UtcNow
        };

        await using var tx = await _db.Database.BeginTransactionAsync();
        _db.Donations.Add(donation);
        await _db.SaveChangesAsync();

        var certificate = new TaxCertificate
        {
            DonationId = donation.DonationId,
            CertificateNumber = $"GOTG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            IssuedAt = DateTime.UtcNow,
            IsIssued = true
        };
        _db.TaxCertificates.Add(certificate);

        if (Input.Frequency == "Recurring")
        {
            _db.DonationSchedules.Add(new DonationSchedule
            {
                DonationId = donation.DonationId,
                Interval = "Monthly",
                StartDate = DateTime.UtcNow.Date,
                NextRunDate = DateTime.UtcNow.Date.AddMonths(1),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        await _audit.WriteAsync(
            "DonationCreated",
            user?.Id,
            nameof(Donation),
            donation.DonationId.ToString(),
            $"Donation {donation.Currency} {donation.Amount:N2}; frequency={donation.Frequency}; anonymous={donation.IsAnonymous}.");

        TempData["ConfirmationMessage"] = $"Thank you. Your donation has been recorded successfully and a certificate has been created for {donation.DonorEmail}.";
        return RedirectToPage("/Certificate", new { id = donation.DonationId });
    }

    private async Task LoadCurrenciesAsync()
    {
        Currencies = await _db.Currencies.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Code).ToListAsync();
    }

    public class DonationInput
    {
        [Required, Display(Name = "Donor name"), StringLength(160)]
        public string DonorName { get; set; } = string.Empty;

        [Required, EmailAddress, Display(Name = "Email address"), StringLength(200)]
        public string DonorEmail { get; set; } = string.Empty;

        [Range(1, 100000000), Display(Name = "Donation amount")]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "ZAR";

        [Required, Display(Name = "Donation type")]
        public string Frequency { get; set; } = "One-time";

        [Display(Name = "Keep my donation anonymous publicly")]
        public bool IsAnonymous { get; set; }
    }
}
