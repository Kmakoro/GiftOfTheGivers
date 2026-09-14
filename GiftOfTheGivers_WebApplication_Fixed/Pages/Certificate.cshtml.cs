using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages;

public class CertificateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CertificateModel(ApplicationDbContext db) => _db = db;

    public Donation? Donation { get; private set; }
    public string? ConfirmationMessage { get; private set; }

    public async Task OnGetAsync(int id)
    {
        ConfirmationMessage = TempData["ConfirmationMessage"] as string;
        Donation = await _db.Donations
            .Include(d => d.TaxCertificate)
            .Include(d => d.Schedule)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DonationId == id);
    }
}
