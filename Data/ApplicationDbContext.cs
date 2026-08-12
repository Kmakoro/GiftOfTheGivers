using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ReliefProject> ReliefProjects => Set<ReliefProject>();
    public DbSet<ProjectUpdate> ProjectUpdates => Set<ProjectUpdate>();
    public DbSet<Volunteer> Volunteers => Set<Volunteer>();
    public DbSet<VolunteerAssignment> VolunteerAssignments => Set<VolunteerAssignment>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<TaxCertificate> TaxCertificates => Set<TaxCertificate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Data integrity: no duplicate volunteer emails, no duplicate donation refs.
        builder.Entity<Volunteer>()
            .HasIndex(v => v.Email)
            .IsUnique();

        builder.Entity<Donation>()
            .HasIndex(d => d.Reference)
            .IsUnique();

        // Efficiency: the reporting queries filter/sort on these columns.
        builder.Entity<Donation>().HasIndex(d => d.DonatedAt);
        builder.Entity<Donation>().HasIndex(d => new { d.ReliefProjectId, d.DonatedAt });
        builder.Entity<ProjectUpdate>().HasIndex(p => new { p.ReliefProjectId, p.PostedAt });
        builder.Entity<Volunteer>().HasIndex(v => v.RegisteredAt);

        // A volunteer can only be placed on the same project once.
        builder.Entity<VolunteerAssignment>()
            .HasIndex(a => new { a.VolunteerId, a.ReliefProjectId })
            .IsUnique();

        builder.Entity<TaxCertificate>()
            .HasIndex(t => t.CertificateNumber)
            .IsUnique();

        builder.Entity<TaxCertificate>()
            .HasOne(t => t.Donation)
            .WithOne(d => d.TaxCertificate)
            .HasForeignKey<TaxCertificate>(t => t.DonationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Donation>()
            .HasOne(d => d.ReliefProject)
            .WithMany(p => p.Donations)
            .HasForeignKey(d => d.ReliefProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<VolunteerAssignment>()
            .HasOne(a => a.ReliefProject)
            .WithMany(p => p.Assignments)
            .HasForeignKey(a => a.ReliefProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProjectUpdate>()
            .HasOne(u => u.ReliefProject)
            .WithMany(p => p.Updates)
            .HasForeignKey(u => u.ReliefProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
