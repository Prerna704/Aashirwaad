# Aashirwad

An ASP.NET Core MVC hospital-management application using MongoDB.

## Requirements

- .NET 10 SDK
- A MongoDB connection string

## Run locally

1. Copy `appsettings.example.json` to `appsettings.json`.
2. Set `ConnectionStrings:MongoDB` to your MongoDB connection string. Set `Email:SenderEmail` and `Email:AppPassword` if email features are configured.
3. From this directory, run:

   ```powershell
   dotnet run
   ```

You can provide settings through environment variables instead:

- `ConnectionStrings__MongoDB`
- `Email__SenderEmail`
- `Email__AppPassword`
- `MONGODB_URI` (Node.js password-reset service, if used)
- `SMTP_USER` and `SMTP_PASSWORD` (Node.js password-reset service, if used)

Do not commit `appsettings.json`, credentials, or the local `temp_keys` directory.
