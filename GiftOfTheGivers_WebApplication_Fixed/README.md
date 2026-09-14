# Gift of the Givers Web Application

A C# ASP.NET Core Razor Pages web application for coordinating humanitarian donations, volunteers and relief projects.

## Main features
- Donor registration and secure login
- Employee role and protected employee portal
- Guest and anonymous donations
- One-time and recurring donations
- ZAR, USD and EUR donation records
- Printable donation tax certificates
- Volunteer registration and employee volunteer list
- Relief project management and public project updates
- Donation history, audit activity and contact enquiries
- Responsive desktop, tablet and mobile interface

## Run on Windows
1. Extract the project folder.
2. Run `RESET_DATABASE.cmd` once when starting this updated version.
3. Run `RUN_WINDOWS.cmd`.
4. Open `http://localhost:5098`.

## Local test accounts
Employee: `employee@giftofthegivers.org` / `Employee@123`

Donor: `donor@example.com` / `Donor@123!`

Local development uses SQLite. The application can be configured for SQL Server/Azure SQL through `appsettings`.

## Compatibility note

The employee dashboard calculates donation totals in application memory after retrieving ZAR amounts. This keeps the dashboard compatible with both SQLite during local development and SQL Server/Azure SQL in deployment.
