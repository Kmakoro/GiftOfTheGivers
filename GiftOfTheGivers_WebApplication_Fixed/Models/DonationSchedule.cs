using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models;

public class DonationSchedule
{
    public int DonationScheduleId { get; set; }

    public int DonationId { get; set; }
    public Donation Donation { get; set; } = null!;

    [Required, StringLength(30)]
    public string Interval { get; set; } = "Monthly";

    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime NextRunDate { get; set; } = DateTime.UtcNow.Date.AddMonths(1);
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
