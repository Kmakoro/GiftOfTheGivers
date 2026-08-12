using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Pages.Employee;

[Authorize(Roles = DbSeeder.EmployeeRole)]
public class DashboardModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty]
    public UpdateInput Input { get; set; } = new();

    public List<ReliefProject> Projects { get; private set; } = new();
    public List<Volunteer> Volunteers { get; private set; } = new();
    public List<Donation> Donations { get; private set; } = new();
    public decimal TotalRaised { get; private set; }
    public SelectList ProjectOptions { get; private set; } = new(Enumerable.Empty<ReliefProject>());

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostPostUpdateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        _db.ProjectUpdates.Add(new ProjectUpdate
        {
            ReliefProjectId = Input.ReliefProjectId,
            Title = Input.Title,
            Body = Input.Body,
            PostedAt = DateTime.UtcNow,
            PostedByUserId = _userManager.GetUserId(User)
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = "Project update published.";
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Projects = await _db.ReliefProjects.AsNoTracking().OrderBy(p => p.Name).ToListAsync();

        Volunteers = await _db.Volunteers.AsNoTracking()
            .OrderByDescending(v => v.RegisteredAt)
            .Take(25)
            .ToListAsync();

        Donations = await _db.Donations.AsNoTracking()
            .Include(d => d.ReliefProject)
            .OrderByDescending(d => d.DonatedAt)
            .Take(25)
            .ToListAsync();

        TotalRaised = await _db.Donations.SumAsync(d => (decimal?)d.Amount) ?? 0m;
        ProjectOptions = new SelectList(Projects, nameof(ReliefProject.Id), nameof(ReliefProject.Name));
    }

    public class UpdateInput
    {
        [Display(Name = "Relief project")]
        [Range(1, int.MaxValue, ErrorMessage = "Select a project.")]
        public int ReliefProjectId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        [Display(Name = "Update")]
        public string Body { get; set; } = string.Empty;
    }
}
