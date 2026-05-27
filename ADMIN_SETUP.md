# Security Guide: Admin User Setup

This document explains how to securely set up the default admin user for NobleBank API.

## Overview

The admin user credentials are **NO LONGER hardcoded** in the application. Instead, they must be provided via secure configuration:

- **User Secrets** (for local development)
- **Environment Variables** (for Docker, CI/CD, cloud deployments)
- **Azure Key Vault** (for production Azure deployments)

By default, admin seeding is **disabled** in `appsettings.json` to prevent accidental creation of well-known credentials in production.

## Local Development Setup

### Using dotnet user-secrets (Recommended)

1. **Initialize user-secrets for the project** (if not already done):
   ```powershell
   cd NobleBank.API
   dotnet user-secrets init
   ```

2. **Set the admin credentials**:
   ```powershell
   dotnet user-secrets set "AdminSeeder:Email" "admin@noblebank.local"
   dotnet user-secrets set "AdminSeeder:Password" "YourStrongPassword123!@#"
   dotnet user-secrets set "AdminSeeder:Disabled" "false"
   ```

   **Important**: Choose a strong password that meets these requirements:
   - At least 8 characters
   - At least one uppercase letter
   - At least one lowercase letter
   - At least one digit
   - At least one special character (!@#$%^&*)

3. **Verify secrets are set**:
   ```powershell
   dotnet user-secrets list
   ```

4. **Run the application**:
   ```powershell
   dotnet run
   ```

The admin user will be created automatically on startup.

### Alternative: Environment Variables

Set environment variables before running the app:

```powershell
$env:AdminSeeder__Email = "admin@noblebank.local"
$env:AdminSeeder__Password = "YourStrongPassword123!@#"
$env:AdminSeeder__Disabled = "false"
dotnet run
```

## Production / Staging Deployment

### Disable Admin Seeding (Recommended)

Keep `"AdminSeeder": { "Disabled": true }` in production settings to **prevent automatic admin creation**.

### Manual Admin Setup

If you need an admin account:

1. Create the account via API registration as a regular user
2. Manually add the Administrator role via database or admin panel
3. Or, set up a one-time bootstrapping process outside the application

### Azure Key Vault

1. **Store secrets in Azure Key Vault**:
   ```bash
   az keyvault secret set --vault-name MyKeyVault --name AdminSeeder--Email --value "admin@company.com"
   az keyvault secret set --vault-name MyKeyVault --name AdminSeeder--Password --value "YourStrongPassword123!@#"
   az keyvault secret set --vault-name MyKeyVault --name AdminSeeder--Disabled --value "false"
   ```

2. **Configure App Service to use Key Vault**:
   - Add Managed Identity to the App Service
   - Grant the identity access to Key Vault
   - Azure configuration will automatically inject the secrets

## Docker / Container Deployments

Pass credentials as environment variables:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# ... other Dockerfile instructions ...

ENV AdminSeeder__Email="admin@noblebank.com"
ENV AdminSeeder__Password="YourStrongPassword123!@#"
ENV AdminSeeder__Disabled="false"

ENTRYPOINT ["dotnet", "NobleBank.API.dll"]
```

Or via docker-compose:

```yaml
services:
  api:
	environment:
	  - AdminSeeder__Email=admin@noblebank.com
	  - AdminSeeder__Password=YourStrongPassword123!@#
	  - AdminSeeder__Disabled=false
```

## Configuration Schema

| Setting | Type | Required | Description |
|---------|------|----------|-------------|
| `AdminSeeder:Email` | string | No | Email of the admin user. Leave empty to skip seeding. |
| `AdminSeeder:Password` | string | No | Password for the admin user. Must meet Identity password policy. |
| `AdminSeeder:Disabled` | bool | No | If true, skip admin seeding even if credentials are provided. Default: false in production, false in tests. |

## Seeding Behavior

The seeder runs on application startup:

1. **Roles are always created** if they don't exist (Administrator, User)
2. **Admin user is created only if**:
   - `Email` and `Password` are both configured
   - `Disabled` is not true
   - The user doesn't already exist (idempotent)

If the admin user already exists, no action is taken.

## Troubleshooting

### Admin user not being created

Check the application logs:
- `"Admin user seeding is disabled or not configured"` → Set credentials via user-secrets or environment variables
- `"Admin user {Email} already exists"` → User was already created; no action needed
- `"Failed to create admin user"` → Check password meets Identity requirements and logs for details

### Password validation errors

If admin creation fails with validation errors, ensure the password:
- ✓ Contains uppercase letters (A-Z)
- ✓ Contains lowercase letters (a-z)
- ✓ Contains digits (0-9)
- ✓ Contains special characters (!@#$%^&*)
- ✓ Is at least 8 characters long

### User-secrets not being picked up

1. Ensure the project was initialized: `dotnet user-secrets init`
2. Check that you're running from the `NobleBank.API` directory
3. Restart the application after setting secrets

## Security Best Practices

1. **Never commit credentials** to version control
2. **Use strong passwords** that meet the policy requirements
3. **Rotate credentials regularly** in production
4. **Use Azure Key Vault** for sensitive configuration in cloud deployments
5. **Disable admin seeding** in production (`Disabled: true`)
6. **Monitor logs** for failed login attempts and credential misuse
7. **Consider implementing MFA** for admin accounts

## Integration Testing

Tests automatically provide admin credentials via the test configuration:
- Email: `admin@test.noblebank.com`
- Password: `TestAdmin123!@#`
- Disabled: `false`

This is configured in `CustomWebAppFactory` and does not interfere with production deployments.
