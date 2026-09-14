using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models;

public class ReliefProject
{
    public int ReliefProjectId { get; set; }

    public string? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }

    [Required, StringLength(180)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(180)]
    public string Location { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Status { get; set; } = "Planning";

    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public ICollection<ProjectUpdate> Updates { get; set; } = new List<ProjectUpdate>();
}
