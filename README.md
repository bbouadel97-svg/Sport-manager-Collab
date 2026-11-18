# Sport Manager

Sport Manager contains an ASP.NET Core/Blazor backend backed by MySQL and a Flutter mobile client.

## Database

The backend reads the `SportManager` connection string from `TeamManager/appsettings.json` (overridden by `appsettings.Development.json` in Development). The existing `sportmanager.db` SQLite file is not used by the current backend.

At startup the backend creates the application tables when needed. It also creates the `users` and `revoked_tokens` tables, then adds nullable ownership columns to teams, players, matches, and injuries. Existing demo records stay shared and read-only; newly created records belong to the authenticated user. Positions remain a shared reference catalog. User passwords are stored as ASP.NET Core password hashes, never as plain text.

## Start the backend

Configure a private JWT signing key of at least 32 bytes in .NET user-secrets:

```powershell
dotnet user-secrets init --project TeamManager\TeamManager.csproj
dotnet user-secrets set "Jwt:SigningKey" "<at-least-32-random-bytes>" --project TeamManager\TeamManager.csproj
```

Then start the API so devices on the local network can reach it:

```powershell
dotnet run --project TeamManager\TeamManager.csproj --urls http://0.0.0.0:5084
```

The API uses the shared MySQL database configured by the connection string. For deployment, use HTTPS and provide the database credentials and JWT key through a secret store or environment variables.

## Start the Flutter client

```powershell
cd sport_manager_mobile
flutter pub get
flutter run
```

The Android emulator uses `http://10.0.2.2:5084` by default. For a physical device or another client, configure `API_BASE_URL` with the backend machine's reachable address:

```powershell
flutter run --dart-define=API_BASE_URL=http://<backend-ip>:5084
```

## Authentication

The Flutter app provides registration and login screens and keeps its one-hour JWT in platform secure storage. The Blazor site uses the same accounts with an eight-hour authentication cookie.

- `POST /api/auth/register` creates an account; passwords must contain at least 8 characters.
- `POST /api/auth/login` returns a one-hour JWT.
- `POST /api/auth/logout` revokes the authenticated JWT until its expiration.
- The teams and players API endpoints require a valid Bearer token. Team create/update/delete requests are restricted to the owner.
- The Blazor pages require login. Shared demo teams, players, matches, and injuries can be viewed but not changed.

Teams, players, matches, and injuries created after login are visible only to their owner. Legacy demo records remain visible to all authenticated users.

## Validation

```powershell
dotnet build TeamManager\TeamManager.csproj
cd sport_manager_mobile
flutter analyze
flutter test
```
