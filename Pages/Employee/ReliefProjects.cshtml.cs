using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Pages.Employee;

public class ReliefProjectsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _audit;

    public ReliefProjectsModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IAuditService audit)
    {
        _db = db;
        _userManager = userManager;
        _audit = audit;
    }

    [BindProperty] public ProjectInput Input { get; set; } = new();
    public List<ReliefProject> Projects { get; private set; } = new();
    public List<ProjectStatusReference> Statuses { get; private set; } = new();

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        await LoadStatusesAsync();
        var validStatuses = Statuses.Select(s => s.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!validStatuses.Contains(Input.Status))
            ModelState.AddModelError("Input.Status", "Invalid project status.");

        if (!ModelState.IsValid)
        {
            await LoadProjectsAsync();
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        var project = new ReliefProject
        {
            CreatedByUserId = user?.Id,
            Title = Input.Title.Trim(),
            Location = Input.Location.Trim(),
            Description = Input.Description.Trim(),
            Status = Input.Status,
            StartDate = Input.StartDate.Date,
            LastUpdated = DateTime.UtcNow
        };
        _db.ReliefProjects.Add(project);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync("ReliefProjectCreated", user?.Id, nameof(ReliefProject), project.ReliefProjectId.ToString(), project.Title);
        TempData["Message"] = "Relief project created successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddUpdateAsync(int reliefProjectId, string updateText)
    {
        if (string.IsNullOrWhiteSpace(updateText))
        {
            TempData["Message"] = "Please enter an update before posting.";
            return RedirectToPage();
        }

        var project = await _db.ReliefProjects.FindAsync(reliefProjectId);
        if (project is null) return NotFound();
        var user = await _userManager.GetUserAsync(User);
        var update = new ProjectUpdate
        {
            ReliefProjectId = reliefProjectId,
            CreatedByUserId = user?.Id,
            UpdateText = updateText.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _db.ProjectUpdates.Add(update);
        project.LastUpdated = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.WriteAsync("ProjectUpdatePosted", user?.Id, nameof(ProjectUpdate), update.ProjectUpdateId.ToString(), $"ReliefProjectId={reliefProjectId}");
        TempData["Message"] = "Project update posted successfully.";
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        await LoadStatusesAsync();
        await LoadProjectsAsync();
    }

    private async Task LoadStatusesAsync()
    {
        Statuses = await _db.ProjectStatuses.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.SortOrder).ToListAsync();
    }

    private async Task LoadProjectsAsync()
    {
        Projects = await _db.ReliefProjects
            .Include(p => p.Updates)
            .AsNoTracking()
            .OrderByDescending(p => p.LastUpdated)
            .ToListAsync();
    }

    public class ProjectInput
    {
        [Required, StringLength(180)] public string Title { get; set; } = string.Empty;
        [Required, StringLength(180)] public string Location { get; set; } = string.Empty;
        [Required, StringLength(2000)] public string Description { get; set; } = string.Empty;
        [Required] public string Status { get; set; } = "Active";
        [Required, DataType(DataType.Date), Display(Name = "Start date")] public DateTime StartDate { get; set; } = DateTime.Today;
    }
}
