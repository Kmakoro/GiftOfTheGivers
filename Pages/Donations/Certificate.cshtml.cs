using GiftOfTheGivers.Data;
using GiftOfTheGivers.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages.Donations;

/// <summary>Streams the placeholder tax certificate PDF for a donation reference.</summary>
public class CertificateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ITaxCertificatePdfService _pdf;

    public CertificateModel(ApplicationDbContext db, ITaxCertificatePdfService pdf)
    {
        _db = db;
        _pdf = pdf;
    }

    public async Task<IActionResult> OnGetAsync(string reference)
    {
        var donation = await _db.Donations
            .AsNoTracking()
            .Include(d => d.ReliefProject)
            .Include(d => d.TaxCertificate)
            .FirstOrDefaultAsync(d => d.Reference == reference);

        if (donation?.TaxCertificate is null)
            return NotFound();

        var bytes = _pdf.Render(donation, donation.TaxCertificate);
        return File(bytes, "application/pdf", $"tax-certificate-{donation.TaxCertificate.CertificateNumber}.pdf");
    }
}
