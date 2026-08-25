using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages.Donor;

public class DashboardModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    public DashboardModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager) { _db = db; _userManager = userManager; }

    public ApplicationUser? CurrentUser { get; private set; }
    public List<Donation> Donations { get; private set; } = new();
    public decimal TotalDonated { get; private set; }
    public int RecurringCount { get; private set; }

    public async Task OnGetAsync()
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser is null) return;
        Donations = await _db.Donations.AsNoTracking().Include(d => d.TaxCertificate).Where(d => d.UserId == CurrentUser.Id).OrderByDescending(d => d.DonationDate).ToListAsync();
        TotalDonated = Donations.Sum(d => d.Amount);
        RecurringCount = Donations.Count(d => d.Frequency == "Recurring");
    }
}
