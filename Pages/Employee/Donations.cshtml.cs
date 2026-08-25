using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages.Employee;

public class DonationsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DonationsModel(ApplicationDbContext db) => _db = db;
    public List<Donation> Donations { get; private set; } = new();

    public async Task OnGetAsync() => Donations = await _db.Donations
        .Include(d => d.TaxCertificate)
        .Include(d => d.Schedule)
        .AsNoTracking()
        .OrderByDescending(d => d.DonationDate)
        .Take(200)
        .ToListAsync();
}
