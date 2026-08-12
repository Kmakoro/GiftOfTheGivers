using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<ReliefProject> Projects { get; private set; } = new();
    public List<ProjectUpdate> RecentUpdates { get; private set; } = new();
    public int ActiveProjects { get; private set; }
    public int VolunteerCount { get; private set; }
    public int DonationCount { get; private set; }
    public decimal TotalRaised { get; private set; }

    public async Task OnGetAsync()
    {
        Projects = await _db.ReliefProjects
            .AsNoTracking()
            .OrderByDescending(p => p.Status == ProjectStatus.Active)
            .ThenByDescending(p => p.StartDate)
            .Take(3)
            .ToListAsync();

        RecentUpdates = await _db.ProjectUpdates
            .AsNoTracking()
            .Include(u => u.ReliefProject)
            .OrderByDescending(u => u.PostedAt)
            .Take(4)
            .ToListAsync();

        ActiveProjects = await _db.ReliefProjects.CountAsync(p => p.Status == ProjectStatus.Active);
        VolunteerCount = await _db.Volunteers.CountAsync();
        DonationCount = await _db.Donations.CountAsync();
        TotalRaised = await _db.Donations.SumAsync(d => (decimal?)d.Amount) ?? 0m;
    }
}
