using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages.Employee;

public class AuditLogsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public AuditLogsModel(ApplicationDbContext db) => _db = db;

    public List<AuditLog> Logs { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Logs = await _db.AuditLogs.AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(100)
            .ToListAsync();
    }
}
