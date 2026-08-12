using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace GiftOfTheGivers.Models;

/// <summary>Application user (Employee or Donor) backed by ASP.NET Identity.</summary>
public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public enum ProjectStatus
{
    Planned = 0,
    Active = 1,
    OnHold = 2,
    Completed = 3
}

/// <summary>A disaster relief operation run by the foundation.</summary>
public class ReliefProject
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    [Display(Name = "Project name")]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Start date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

    [Display(Name = "End date")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Funding goal")]
    public decimal FundingGoal { get; set; }

    public ICollection<ProjectUpdate> Updates { get; set; } = new List<ProjectUpdate>();
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public ICollection<VolunteerAssignment> Assignments { get; set; } = new List<VolunteerAssignment>();
}

/// <summary>A status note posted by an employee against a relief project.</summary>
public class ProjectUpdate
{
    public int Id { get; set; }

    public int ReliefProjectId { get; set; }
    public ReliefProject? ReliefProject { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Body { get; set; } = string.Empty;

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string? PostedByUserId { get; set; }
    public ApplicationUser? PostedBy { get; set; }
}

/// <summary>Someone offering their time and skills. May or may not have a login.</summary>
public class Volunteer
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Phone, MaxLength(30)]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Required, MaxLength(300)]
    [Display(Name = "Skills (comma separated)")]
    public string Skills { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Availability { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? City { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<VolunteerAssignment> Assignments { get; set; } = new List<VolunteerAssignment>();
}

/// <summary>Join entity placing a volunteer on a relief project.</summary>
public class VolunteerAssignment
{
    public int Id { get; set; }

    public int VolunteerId { get; set; }
    public Volunteer? Volunteer { get; set; }

    public int ReliefProjectId { get; set; }
    public ReliefProject? ReliefProject { get; set; }

    [MaxLength(100)]
    public string? Role { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

public enum DonationFrequency
{
    OneTime = 0,
    Monthly = 1
}

public enum Currency
{
    ZAR = 0,
    USD = 1,
    EUR = 2
}

/// <summary>A monetary contribution. Donors may be registered or anonymous guests.</summary>
public class Donation
{
    public int Id { get; set; }

    [Required, MaxLength(40)]
    [Display(Name = "Reference")]
    public string Reference { get; set; } = string.Empty;

    [Range(1, 10_000_000)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public Currency Currency { get; set; } = Currency.ZAR;

    public DonationFrequency Frequency { get; set; } = DonationFrequency.OneTime;

    [Required, MaxLength(120)]
    [Display(Name = "Donor name")]
    public string DonorName { get; set; } = "Anonymous";

    [EmailAddress, MaxLength(200)]
    [Display(Name = "Donor email")]
    public string? DonorEmail { get; set; }

    [Display(Name = "Donate anonymously")]
    public bool IsAnonymous { get; set; }

    public DateTime DonatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public int? ReliefProjectId { get; set; }
    public ReliefProject? ReliefProject { get; set; }

    public TaxCertificate? TaxCertificate { get; set; }
}

/// <summary>Section 18A style placeholder certificate generated for every donation.</summary>
public class TaxCertificate
{
    public int Id { get; set; }

    public int DonationId { get; set; }
    public Donation? Donation { get; set; }

    [Required, MaxLength(40)]
    public string CertificateNumber { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
}
