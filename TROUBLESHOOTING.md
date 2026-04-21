# Troubleshooting Guide

Common issues and solutions for the Enterprise Project Management System.

## Backend Issues

### SQL Server Connection Error

**Error:** `Cannot connect to SQL Server`

**Solutions:**
1. Verify SQL Server is installed and running
2. Check connection string in `appsettings.json`
3. Ensure database `EnterpriseDb` exists
4. Verify Windows Authentication is enabled

### Port Already in Use

**Error:** `Address already in use. Port: 5000`

**Solutions:**
1. Find process using port 5000:
   ```bash
   netstat -ano | findstr :5000
   ```
2. Kill the process:
   ```bash
   taskkill /PID <PID> /F
   ```
3. Or change port in `launchSettings.json`

### Database Migration Failed

**Error:** `Failed to execute migrations`

**Solutions:**
1. Reset database:
   ```bash
   dotnet ef database drop
   dotnet ef database update
   ```
2. Check migration files for syntax errors
3. Ensure SQL Server is running and accessible

### Swagger UI Not Loading

**Error:** `Swagger UI returns 404`

**Solutions:**
1. Ensure `app.UseSwagger()` and `app.UseSwaggerUI()` are in `Program.cs`
2. Check that the port is correct (default: 5000)
3. Verify firewall isn't blocking access

## Frontend Issues

### npm Installation Fails

**Error:** `npm ERR! code ERESOLVE`

**Solutions:**
1. Clear npm cache:
   ```bash
   npm cache clean --force
   ```
2. Delete `node_modules` folder
3. Delete `package-lock.json`
4. Run `npm install` again

### Port 3000 Already in Use

**Error:** `Something is already running on port 3000`

**Solutions:**
1. React will ask to use another port - choose yes
2. Or kill the process using port 3000:
   ```bash
   netstat -ano | findstr :3000
   taskkill /PID <PID> /F
   ```

### API Connection Error

**Error:** `Failed to load projects. Please ensure the API is running.`

**Solutions:**
1. Verify backend API is running on `http://localhost:5000`
2. Check `.env` file for correct `REACT_APP_API_URL`
3. Enable CORS in backend if accessing from different origin
4. Check browser console for detailed error messages

### Build Fails with Module Not Found

**Error:** `Module not found: Error: Can't resolve './App.css'`

**Solutions:**
1. Ensure `App.css` exists in `src` folder
2. Clear build cache:
   ```bash
   npm run build -- --reset-cache
   ```
3. Delete `node_modules` and reinstall

### Cannot Read Property of Undefined

**Error:** `Cannot read properties of undefined (reading 'data')`

**Solutions:**
1. Check API response format
2. Add null checks in your code:
   ```javascript
   const data = response?.data || [];
   ```
3. Use optional chaining operator (`?.`)

## Full-Stack Issues

### API Works but Frontend Shows No Data

**Solution:**
1. Open browser DevTools (F12)
2. Check Network tab for API requests
3. Verify response status is 200
4. Check response payload structure matches expected format

### CORS Error When Calling API

**Error:** `Access to XMLHttpRequest blocked by CORS policy`

**Solution - Add CORS to Backend:**

In `Program.cs`, add before `app.MapControllers()`:

```csharp
app.UseCors(builder =>
{
    builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
});

app.MapControllers();
```

### Validation Errors Not Showing

**Solution:**
1. Verify client-side validation in React
2. Check server validation attributes on model
3. Ensure error response is properly formatted
4. Check browser console for JavaScript errors

## Performance Issues

### Slow API Responses

**Solutions:**
1. Add indexes to database columns (especially `Id`)
2. Check SQL queries with SQL Profiler
3. Add caching for frequently accessed data
4. Optimize Entity Framework queries (use `.AsNoTracking()`)

### Large Bundle Size

**Solution:**
1. Run production build:
   ```bash
   npm run build
   ```
2. Analyze bundle:
   ```bash
   npm install -g source-map-explorer
   source-map-explorer 'build/static/js/*.js'
   ```
3. Consider code splitting and lazy loading

## Getting More Help

1. Check existing GitHub Issues
2. Review the Architecture documentation
3. Enable debug logging in backend
4. Check browser console and DevTools
5. Review Entity Framework logs

## Debug Logging

Enable detailed logging in `Program.cs`:

```csharp
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

Or in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Debug"
    }
  }
}
```

## Reset Everything

If you encounter persistent issues:

### Backend
```bash
# Clean build
dotnet clean
dotnet build

# Reset database
dotnet ef database drop
dotnet ef database update
```

### Frontend
```bash
# Clean install
rm -r node_modules package-lock.json
npm install
npm start
```
