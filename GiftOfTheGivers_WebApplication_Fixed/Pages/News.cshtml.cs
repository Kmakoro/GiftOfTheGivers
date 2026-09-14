using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages;

public class NewsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public NewsModel(ApplicationDbContext db) => _db = db;

    public List<ProjectUpdate> Updates { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Updates = await _db.ProjectUpdates
            .Include(u => u.ReliefProject)
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Take(12)
            .ToListAsync();
    }
}
