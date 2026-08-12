# Gift of the Givers Foundation — ASP.NET Core Razor Pages prototype

Part 1 prototype (Section C) for the Disaster Alleviation Foundation project.
Target framework: **.NET 8** • Razor Pages • EF Core + Azure SQL • ASP.NET Identity • Bootstrap 5.

## Running locally

```bash
cd GiftOfTheGivers
dotnet restore
dotnet tool install --global dotnet-ef        # once, if not installed
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

`Program.cs` calls `Database.MigrateAsync()` on start-up, so once a migration exists the
database, roles and sample projects are created automatically.

Seeded employee login (change in `appsettings.json`):

- Email: `employee@giftofthegivers.org`
- Password: `Employee#123`

Donors register themselves at `/Identity/Account/Register`, or donate without an account.

## Pages

| Route | Purpose |
|---|---|
| `/` | Branded landing page with live statistics and latest field updates |
| `/About` | Foundation background |
| `/Projects` | All relief projects with their updates |
| `/Donations/Index` | One-time or recurring donation, ZAR/USD/EUR, anonymous option |
| `/Donations/ThankYou` | Confirmation with certificate number |
| `/Donations/Certificate` | Downloadable placeholder tax certificate PDF (QuestPDF) |
| `/Volunteers/Register` | Volunteer interest form (name, skills, availability) |
| `/Employee/Dashboard` | Employee-only: post project updates, view volunteer sign-ups and donations |
| `/Contact` | Contact form |
| `/Identity/Account/*` | Login, register, manage (ASP.NET Identity default UI) |

## Roles

- **Employee** — logs in, posts updates on ongoing relief projects, views volunteer sign-ups
  and donation records. Enforced by the `EmployeeOnly` policy on the `/Employee` folder.
- **Donor** — registers/logs in, or donates as an anonymous guest.

## Database

`Docs/schema.sql` is the Azure SQL script for the six domain tables, keys, constraints and
indexes. It mirrors what EF Core generates from `Models/Entities.cs` and
`Data/ApplicationDbContext.cs`.

Entities: `ReliefProject`, `ProjectUpdate`, `Volunteer`, `VolunteerAssignment`,
`Donation`, `TaxCertificate` (plus Identity's `AspNetUsers` / `AspNetRoles`).

Relationships:

```text
AspNetUsers 1──* Donations            AspNetUsers 1──0..1 Volunteers
AspNetUsers 1──* ProjectUpdates       ReliefProjects 1──* ProjectUpdates
ReliefProjects 1──* Donations         ReliefProjects 1──* VolunteerAssignments *──1 Volunteers
Donations 1──1 TaxCertificates
```

## Deploying to Azure App Service

1. Create an Azure SQL Database and copy its ADO.NET connection string.
2. Create an App Service (Windows or Linux, .NET 8 runtime).
3. In App Service → Configuration → Connection strings, add `DefaultConnection` of type
   `SQLAzure` with that value.
4. Add the App Service outbound IPs (or "Allow Azure services") to the SQL server firewall.
5. Publish: `dotnet publish -c Release` then deploy via Visual Studio publish profile,
   `az webapp deploy`, or a GitHub Actions workflow.

## Note on written sections

Sections A (Azure Boards), B 2.1/2.3 discussion and C 3.1 are written deliverables that your
group must author yourselves — this repository is the code and schema they refer to.
