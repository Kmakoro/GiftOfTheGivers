using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages.Employee;

public class DashboardModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DashboardModel(ApplicationDbContext db) => _db = db;
    public decimal ZarDonationTotal { get; private set; }
    public int DonationCount { get; private set; }
    public int VolunteerCount { get; private set; }
    public int ActiveProjectCount { get; private set; }
    public List<ReliefProject> RecentProjects { get; private set; } = new();
    public List<VolunteerApplication> RecentVolunteers { get; private set; } = new();
    public List<Donation> RecentDonations { get; private set; } = new();
    public async Task OnGetAsync()
    {
        DonationCount = await _db.Donations.CountAsync();

        // SQLite does not support SUM directly over decimal columns.
        // Load only the matching amounts, then aggregate them in .NET.
        var zarAmounts = await _db.Donations
            .AsNoTracking()
            .Where(d => d.Currency == "ZAR")
            .Select(d => d.Amount)
            .ToListAsync();
        ZarDonationTotal = zarAmounts.Sum();

        VolunteerCount = await _db.VolunteerApplications.CountAsync();
        ActiveProjectCount = await _db.ReliefProjects.CountAsync(p => p.Status == "Active");
        RecentProjects = await _db.ReliefProjects.AsNoTracking().OrderByDescending(p => p.LastUpdated).Take(5).ToListAsync();
        RecentVolunteers = await _db.VolunteerApplications.AsNoTracking().OrderByDescending(v => v.DateSubmitted).Take(5).ToListAsync();
        RecentDonations = await _db.Donations.AsNoTracking().OrderByDescending(d => d.DonationDate).Take(5).ToListAsync();
    }
}
