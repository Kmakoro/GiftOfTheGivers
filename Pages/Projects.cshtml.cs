using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages;

public class ProjectsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public ProjectsModel(ApplicationDbContext db) => _db = db;

    public List<ReliefProject> Projects { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Projects = await _db.ReliefProjects
            .Include(p => p.Updates)
            .AsNoTracking()
            .OrderBy(p => p.Status == "Active" ? 0 : p.Status == "Planning" ? 1 : 2)
            .ThenByDescending(p => p.LastUpdated)
            .ToListAsync();
    }
}
