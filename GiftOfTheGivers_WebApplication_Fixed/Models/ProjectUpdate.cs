using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models;

public class ProjectUpdate
{
    public int ProjectUpdateId { get; set; }

    public int ReliefProjectId { get; set; }
    public ReliefProject ReliefProject { get; set; } = null!;

    public string? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }

    [Required, StringLength(1500)]
    public string UpdateText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
