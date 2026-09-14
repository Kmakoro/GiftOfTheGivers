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
    public int ActiveProjectCount { get; private set; }
    public int VolunteerCount { get; private set; }
    public int DonationCount { get; private set; }

    public async Task OnGetAsync()
    {
        Projects = await _db.ReliefProjects
            .Include(p => p.Updates)
            .AsNoTracking()
            .Where(p => p.Status == "Active" || p.Status == "Planning")
            .OrderByDescending(p => p.LastUpdated)
            .Take(4)
            .ToListAsync();
        ActiveProjectCount = await _db.ReliefProjects.CountAsync(p => p.Status == "Active");
        VolunteerCount = await _db.VolunteerApplications.CountAsync();
        DonationCount = await _db.Donations.CountAsync();
    }
}
