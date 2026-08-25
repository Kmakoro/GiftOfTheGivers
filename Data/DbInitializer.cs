using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.EnsureCreatedAsync();

        foreach (var role in new[] { "Donor", "Employee" })
        {
            if (await roleManager.RoleExistsAsync(role)) continue;
            var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
            if (!roleResult.Succeeded)
                throw new InvalidOperationException($"Unable to create role '{role}': {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
        }

        if (!await db.Currencies.AnyAsync())
        {
            db.Currencies.AddRange(
                new CurrencyReference { Code = "ZAR", DisplayName = "South African Rand", Symbol = "R", IsActive = true },
                new CurrencyReference { Code = "USD", DisplayName = "US Dollar", Symbol = "$", IsActive = true },
                new CurrencyReference { Code = "EUR", DisplayName = "Euro", Symbol = "€", IsActive = true });
        }

        if (!await db.ProjectStatuses.AnyAsync())
        {
            db.ProjectStatuses.AddRange(
                new ProjectStatusReference { Code = "Planning", DisplayName = "Planning", SortOrder = 1, IsActive = true },
                new ProjectStatusReference { Code = "Active", DisplayName = "Active", SortOrder = 2, IsActive = true },
                new ProjectStatusReference { Code = "Completed", DisplayName = "Completed", SortOrder = 3, IsActive = true });
        }
        await db.SaveChangesAsync();

        const string employeeEmail = "employee@giftofthegivers.org";
        var employee = await EnsureAccountAsync(userManager, employeeEmail, "Employee@123", "Thando", "Ndlovu", "Employee", disableLockout: true);

        const string donorEmail = "donor@example.com";
        var donor = await EnsureAccountAsync(userManager, donorEmail, "Donor@123!", "Lerato", "Molefe", "Donor", disableLockout: true);

        if (!await db.ReliefProjects.AnyAsync())
        {
            db.ReliefProjects.AddRange(
                new ReliefProject { CreatedByUserId = employee.Id, Title = "Feed the Hungry", Location = "Gauteng", Description = "Providing meals and food parcels to vulnerable communities in need.", Status = "Active", StartDate = DateTime.UtcNow.Date.AddDays(-30), LastUpdated = DateTime.UtcNow.AddHours(-2) },
                new ReliefProject { CreatedByUserId = employee.Id, Title = "Disaster Relief", Location = "KwaZulu-Natal", Description = "Emergency relief for disaster-affected families, including food, water and essential supplies.", Status = "Active", StartDate = DateTime.UtcNow.Date.AddDays(-18), LastUpdated = DateTime.UtcNow.AddHours(-5) },
                new ReliefProject { CreatedByUserId = employee.Id, Title = "Winter Warmth", Location = "Eastern Cape", Description = "Providing blankets and warm clothing to communities during severe winter conditions.", Status = "Active", StartDate = DateTime.UtcNow.Date.AddDays(-10), LastUpdated = DateTime.UtcNow.AddDays(-1) },
                new ReliefProject { CreatedByUserId = employee.Id, Title = "Water for Life", Location = "Limpopo", Description = "Clean-water response supporting rural communities experiencing water shortages.", Status = "Planning", StartDate = DateTime.UtcNow.Date.AddDays(4), LastUpdated = DateTime.UtcNow.AddDays(-2) });
            await db.SaveChangesAsync();
        }

        if (!await db.ProjectUpdates.AnyAsync())
        {
            var projects = await db.ReliefProjects.OrderBy(p => p.ReliefProjectId).Take(4).ToListAsync();
            foreach (var p in projects)
            {
                db.ProjectUpdates.Add(new ProjectUpdate
                {
                    ReliefProjectId = p.ReliefProjectId,
                    CreatedByUserId = employee.Id,
                    UpdateText = p.Title switch
                    {
                        "Feed the Hungry" => "Food parcels were distributed to families identified by local community partners.",
                        "Disaster Relief" => "Emergency response teams delivered water and hygiene supplies to affected areas.",
                        "Winter Warmth" => "Blankets and winter clothing were prepared for the next community distribution.",
                        _ => "Planning and logistics are underway for the next phase of this relief project."
                    },
                    CreatedAt = DateTime.UtcNow.AddHours(-projects.IndexOf(p) * 4)
                });
            }
            await db.SaveChangesAsync();
        }

        if (!await db.VolunteerApplications.AnyAsync())
        {
            db.VolunteerApplications.AddRange(
                new VolunteerApplication { FullName = "Nomsa Mkhize", Email = "nomsa@example.com", Phone = "+27 72 555 0101", Skills = "First aid, community outreach", Availability = "Weekends", DateSubmitted = DateTime.UtcNow.AddHours(-3) },
                new VolunteerApplication { FullName = "Thabo Khumalo", Email = "thabo@example.com", Phone = "+27 71 555 0102", Skills = "Driving, logistics", Availability = "Flexible", DateSubmitted = DateTime.UtcNow.AddHours(-8) },
                new VolunteerApplication { FullName = "Aisha Patel", Email = "aisha@example.com", Phone = "+27 82 555 0103", Skills = "Administration, IT support", Availability = "Evenings", DateSubmitted = DateTime.UtcNow.AddDays(-1) },
                new VolunteerApplication { FullName = "Michael Brown", Email = "michael@example.com", Phone = "+27 83 555 0104", Skills = "Warehouse support", Availability = "Emergency call-outs", DateSubmitted = DateTime.UtcNow.AddDays(-2) });
            await db.SaveChangesAsync();
        }

        if (!await db.Donations.AnyAsync())
        {
            var donations = new List<Donation>
            {
                new() { UserId = donor.Id, DonorName = "Lerato Molefe", DonorEmail = donorEmail, Amount = 1000, Currency = "ZAR", Frequency = "One-time", IsAnonymous = false, DonationDate = DateTime.UtcNow.AddHours(-2) },
                new() { DonorName = "Anonymous", DonorEmail = "anonymous@example.com", Amount = 500, Currency = "ZAR", Frequency = "One-time", IsAnonymous = true, DonationDate = DateTime.UtcNow.AddHours(-5) },
                new() { DonorName = "John Dlamini", DonorEmail = "john@example.com", Amount = 2000, Currency = "ZAR", Frequency = "Recurring", IsAnonymous = false, DonationDate = DateTime.UtcNow.AddDays(-1) },
                new() { DonorName = "Fatima Khan", DonorEmail = "fatima@example.com", Amount = 750, Currency = "ZAR", Frequency = "One-time", IsAnonymous = false, DonationDate = DateTime.UtcNow.AddDays(-2) },
                new() { UserId = donor.Id, DonorName = "Lerato Molefe", DonorEmail = donorEmail, Amount = 50, Currency = "USD", Frequency = "One-time", IsAnonymous = false, DonationDate = DateTime.UtcNow.AddDays(-3) }
            };
            db.Donations.AddRange(donations);
            await db.SaveChangesAsync();

            foreach (var d in donations)
            {
                db.TaxCertificates.Add(new TaxCertificate
                {
                    DonationId = d.DonationId,
                    CertificateNumber = $"GOTG-{d.DonationDate:yyyyMMdd}-{d.DonationId:0000}",
                    IssuedAt = d.DonationDate.AddMinutes(1),
                    IsIssued = true
                });
                if (d.Frequency == "Recurring")
                {
                    db.DonationSchedules.Add(new DonationSchedule
                    {
                        DonationId = d.DonationId,
                        Interval = "Monthly",
                        StartDate = d.DonationDate.Date,
                        NextRunDate = d.DonationDate.Date.AddMonths(1),
                        IsActive = true,
                        CreatedAt = d.DonationDate
                    });
                }
            }
            await db.SaveChangesAsync();
        }
    }
    private static async Task<ApplicationUser> EnsureAccountAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        bool disableLockout)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow
            };
            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException($"Unable to create account '{email}': {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
        }
        else
        {
            var changed = false;
            if (user.UserName != email) { user.UserName = email; changed = true; }
            if (user.Email != email) { user.Email = email; changed = true; }
            if (!user.EmailConfirmed) { user.EmailConfirmed = true; changed = true; }
            if (string.IsNullOrWhiteSpace(user.FirstName)) { user.FirstName = firstName; changed = true; }
            if (string.IsNullOrWhiteSpace(user.LastName)) { user.LastName = lastName; changed = true; }
            if (changed)
            {
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    throw new InvalidOperationException($"Unable to update account '{email}': {string.Join("; ", updateResult.Errors.Select(e => e.Description))}");
            }

            if (!await userManager.CheckPasswordAsync(user, password))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await userManager.ResetPasswordAsync(user, token, password);
                if (!resetResult.Succeeded)
                    throw new InvalidOperationException($"Unable to reset the password for '{email}': {string.Join("; ", resetResult.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, role);
            if (!addRoleResult.Succeeded)
                throw new InvalidOperationException($"Unable to assign role '{role}' to '{email}': {string.Join("; ", addRoleResult.Errors.Select(e => e.Description))}");
        }

        if (disableLockout)
        {
            await userManager.SetLockoutEnabledAsync(user, false);
            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);
        }

        return user;
    }

}
