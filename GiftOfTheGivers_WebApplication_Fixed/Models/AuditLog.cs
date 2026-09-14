using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models;

public class AuditLog
{
    public long AuditLogId { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    [Required, StringLength(80)]
    public string Action { get; set; } = string.Empty;

    [StringLength(80)]
    public string? EntityName { get; set; }

    [StringLength(80)]
    public string? EntityId { get; set; }

    [StringLength(1000)]
    public string? Details { get; set; }

    [StringLength(64)]
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
