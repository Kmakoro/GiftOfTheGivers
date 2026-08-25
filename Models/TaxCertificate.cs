using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models;

public class TaxCertificate
{
    public int TaxCertificateId { get; set; }

    public int DonationId { get; set; }
    public Donation Donation { get; set; } = null!;

    [Required, StringLength(50)]
    public string CertificateNumber { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public bool IsIssued { get; set; } = true;
}
