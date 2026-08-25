using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<DonationSchedule> DonationSchedules => Set<DonationSchedule>();
    public DbSet<TaxCertificate> TaxCertificates => Set<TaxCertificate>();
    public DbSet<VolunteerApplication> VolunteerApplications => Set<VolunteerApplication>();
    public DbSet<ReliefProject> ReliefProjects => Set<ReliefProject>();
    public DbSet<ProjectUpdate> ProjectUpdates => Set<ProjectUpdate>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<CurrencyReference> Currencies => Set<CurrencyReference>();
    public DbSet<ProjectStatusReference> ProjectStatuses => Set<ProjectStatusReference>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CurrencyReference>()
            .Property(c => c.Code)
            .HasColumnType("char(3)");
        builder.Entity<CurrencyReference>()
            .HasIndex(c => c.DisplayName)
            .IsUnique();

        builder.Entity<ProjectStatusReference>()
            .HasIndex(s => s.SortOrder)
            .IsUnique();

        builder.Entity<Donation>().HasIndex(d => d.DonationDate);
        builder.Entity<Donation>().HasIndex(d => d.UserId);
        builder.Entity<Donation>().HasIndex(d => new { d.Currency, d.DonationDate });
        builder.Entity<Donation>().HasIndex(d => d.Frequency);

        builder.Entity<Donation>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<DonationSchedule>()
            .HasIndex(s => s.DonationId)
            .IsUnique();
        builder.Entity<DonationSchedule>()
            .HasIndex(s => new { s.IsActive, s.NextRunDate });
        builder.Entity<DonationSchedule>()
            .HasOne(s => s.Donation)
            .WithOne(d => d.Schedule)
            .HasForeignKey<DonationSchedule>(s => s.DonationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TaxCertificate>()
            .HasIndex(c => c.CertificateNumber)
            .IsUnique();
        builder.Entity<TaxCertificate>()
            .HasIndex(c => c.DonationId)
            .IsUnique();
        builder.Entity<TaxCertificate>()
            .HasOne(c => c.Donation)
            .WithOne(d => d.TaxCertificate)
            .HasForeignKey<TaxCertificate>(c => c.DonationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<VolunteerApplication>().HasIndex(v => v.Availability);
        builder.Entity<VolunteerApplication>().HasIndex(v => v.Email);
        builder.Entity<VolunteerApplication>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ReliefProject>().HasIndex(r => r.Status);
        builder.Entity<ReliefProject>().HasIndex(r => r.StartDate);
        builder.Entity<ReliefProject>()
            .HasOne(r => r.CreatedByUser)
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ProjectUpdate>()
            .HasIndex(u => new { u.ReliefProjectId, u.CreatedAt });
        builder.Entity<ProjectUpdate>()
            .HasOne(u => u.ReliefProject)
            .WithMany(r => r.Updates)
            .HasForeignKey(u => u.ReliefProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ProjectUpdate>()
            .HasOne(u => u.CreatedByUser)
            .WithMany()
            .HasForeignKey(u => u.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ContactMessage>().HasIndex(c => c.CreatedAt);
        builder.Entity<ContactMessage>().HasIndex(c => c.Email);

        builder.Entity<AuditLog>().HasIndex(a => a.CreatedAt);
        builder.Entity<AuditLog>().HasIndex(a => a.UserId);
        builder.Entity<AuditLog>().HasIndex(a => new { a.Action, a.CreatedAt });
        builder.Entity<AuditLog>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
