using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages.Employee;

public class VolunteersModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public VolunteersModel(ApplicationDbContext db) => _db = db;
    public List<VolunteerApplication> Volunteers { get; private set; } = new();
    public async Task OnGetAsync() => Volunteers = await _db.VolunteerApplications.AsNoTracking().OrderByDescending(v => v.DateSubmitted).ToListAsync();
}
