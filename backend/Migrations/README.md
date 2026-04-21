# Database Migrations

This directory contains Entity Framework Core migrations for database schema management.

## Initial Setup

To create the database with the initial schema:

1. **Using Package Manager Console (Visual Studio):**
   ```powershell
   Update-Database
   ```

2. **Using .NET CLI:**
   ```bash
   dotnet ef database update
   ```

## Creating Migrations

When you modify the model classes:

1. **Using Package Manager Console:**
   ```powershell
   Add-Migration [MigrationName]
   Update-Database
   ```

2. **Using .NET CLI:**
   ```bash
   dotnet ef migrations add [MigrationName]
   dotnet ef database update
   ```

## Resetting Database

To drop and recreate the database:

1. **Using .NET CLI:**
   ```bash
   dotnet ef database drop
   dotnet ef database update
   ```

## Viewing Migrations

To see the migration history:

```bash
dotnet ef migrations list
```

## Notes

- Make sure your SQL Server connection string is correct in `appsettings.json`
- Ensure the database server is running before executing migrations
- Always test migrations on a development database first
