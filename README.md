# Sport Manager (Blazor + EF Core + SQLite)

Simple school project made with Blazor Server and C#.

## What it manages

- Teams
- Players
- Positions
- Matches
- Injuries

## Tech used

- ASP.NET Core Blazor
- Entity Framework Core
- SQLite
- Bootstrap

## Project structure

- `TeamManager/Models` - entity classes
- `TeamManager/Data` - `SportManagerDbContext`
- `TeamManager/Services` - basic business logic and database operations
- `TeamManager/Components/Pages` - Razor pages for each feature

## Run locally

```bash
dotnet restore
dotnet run --project TeamManager/TeamManager.csproj
```

Then open the URL shown in the terminal.

## Notes

- Database file is SQLite: `TeamManager/sportmanager.db`
- Tables are created automatically at app startup with `EnsureCreated()`
- Starter positions are seeded automatically (Goalkeeper, Defender, Midfielder, Forward)

