using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages.Donations;

public class ThankYouModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ThankYouModel(ApplicationDbContext db) => _db = db;

    public Donation? Donation { get; private set; }

    public async Task<IActionResult> OnGetAsync(string reference)
    {
        Donation = await _db.Donations
            .AsNoTracking()
            .Include(d => d.ReliefProject)
            .Include(d => d.TaxCertificate)
            .FirstOrDefaultAsync(d => d.Reference == reference);

        return Donation is null ? NotFound() : Page();
    }
}
