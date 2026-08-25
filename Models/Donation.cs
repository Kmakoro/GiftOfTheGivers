using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiftOfTheGivers.Models;

public class Donation
{
    public int DonationId { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    [Required, StringLength(160)]
    public string DonorName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string DonorEmail { get; set; } = string.Empty;

    [Range(1, 100000000)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required, StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "ZAR";

    [Required, StringLength(20)]
    public string Frequency { get; set; } = "One-time";

    public bool IsAnonymous { get; set; }
    public DateTime DonationDate { get; set; } = DateTime.UtcNow;

    public DonationSchedule? Schedule { get; set; }
    public TaxCertificate? TaxCertificate { get; set; }
}
