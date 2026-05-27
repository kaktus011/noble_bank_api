# Quick Start: Admin Setup

## TL;DR

### Local Development (First Time)

```powershell
cd NobleBank.API
dotnet user-secrets init
dotnet user-secrets set "AdminSeeder:Email" "admin@noblebank.local"
dotnet user-secrets set "AdminSeeder:Password" "Admin123!@#"
dotnet user-secrets set "AdminSeeder:Disabled" "false"
dotnet run
```

### Docker/Container

```dockerfile
ENV AdminSeeder__Email=admin@noblebank.com
ENV AdminSeeder__Password=YourPassword123!@#
ENV AdminSeeder__Disabled=false
```

### Production (Azure Key Vault)

```bash
az keyvault secret set --vault-name MyVault --name AdminSeeder--Email --value "admin@company.com"
az keyvault secret set --vault-name MyVault --name AdminSeeder--Password --value "Strong123!@#"
```

### CI/CD Pipeline

```yaml
# GitHub Actions
env:
  AdminSeeder__Email: ${{ secrets.ADMIN_EMAIL }}
  AdminSeeder__Password: ${{ secrets.ADMIN_PASSWORD }}
  AdminSeeder__Disabled: "false"
```

## Configuration Reference

| Key | Default | Example |
|-----|---------|---------|
| `AdminSeeder:Email` | *(empty)* | `admin@company.com` |
| `AdminSeeder:Password` | *(empty)* | `StrongPassword123!@#` |
| `AdminSeeder:Disabled` | `true` (production) | `false` (development) |

## Password Requirements

- ✓ Minimum 8 characters
- ✓ At least 1 uppercase (A-Z)
- ✓ At least 1 lowercase (a-z)
- ✓ At least 1 digit (0-9)
- ✓ At least 1 special character (!@#$%^&*)

## Troubleshooting

**Admin not being created?**
→ Check logs for "Admin user seeding is disabled"
→ Ensure `Disabled: false` in configuration
→ Verify email and password are set

**Password validation error?**
→ Ensure password meets all requirements above
→ Try: `Test@Password123!#`

**Can't find user-secrets?**
→ Initialize first: `dotnet user-secrets init`
→ Run from `NobleBank.API` directory
→ Restart app after setting secrets
