using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Data;

public static class DbSeeder
{
    public const string EmployeeRole = "Employee";
    public const string DonorRole = "Donor";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        foreach (var role in new[] { EmployeeRole, DonorRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var email = config["SeedUsers:EmployeeEmail"] ?? "employee@giftofthegivers.org";
        var password = config["SeedUsers:EmployeePassword"] ?? "Employee#123";

        if (await userManager.FindByEmailAsync(email) is null)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Relief",
                LastName = "Coordinator"
            };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, EmployeeRole);
        }

        if (!await db.ReliefProjects.AnyAsync())
        {
            db.ReliefProjects.AddRange(
                new ReliefProject
                {
                    Name = "KwaZulu-Natal Flood Response",
                    Location = "Durban, KZN",
                    Description = "Emergency food, water purification and temporary shelter for families displaced by flooding.",
                    Status = ProjectStatus.Active,
                    FundingGoal = 2_500_000m,
                    StartDate = DateTime.UtcNow.Date.AddDays(-30)
                },
                new ReliefProject
                {
                    Name = "Eastern Cape Drought Relief",
                    Location = "Makhanda, Eastern Cape",
                    Description = "Borehole drilling, water tankering and livestock feed for drought-hit rural communities.",
                    Status = ProjectStatus.Active,
                    FundingGoal = 1_200_000m,
                    StartDate = DateTime.UtcNow.Date.AddDays(-90)
                },
                new ReliefProject
                {
                    Name = "Winter Warmth Campaign",
                    Location = "Nationwide",
                    Description = "Blankets, jackets and hot meals distributed through partner shelters during winter.",
                    Status = ProjectStatus.Planned,
                    FundingGoal = 750_000m,
                    StartDate = DateTime.UtcNow.Date.AddDays(14)
                });

            await db.SaveChangesAsync();
        }
    }
}
