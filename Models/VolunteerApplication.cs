using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models;

public class VolunteerApplication
{
    public int VolunteerApplicationId { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    [Required, StringLength(160)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(40)]
    public string? Phone { get; set; }

    [Required, StringLength(500)]
    public string Skills { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Availability { get; set; } = string.Empty;

    public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;
}
