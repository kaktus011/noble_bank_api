using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NobleBank.Application.Common.Exceptions;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;
using NobleBank.Infrastructure.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NobleBank.Infrastructure.Identity
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(IOptions<JwtSettings> settings, UserManager<ApplicationUser> userManager)
        {
            _settings = settings.Value;
            _userManager = userManager;
        }

        public string? GetUserIdFromToken(string token)
        {
            ClaimsPrincipal? principal = ValidateToken(token);
            if (principal is null)
            {
                return null;
            }

            return principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public Guid? GetSessionIdFromToken(string token)
        {
            ClaimsPrincipal? principal = ValidateToken(token);
            string? sid = principal?.FindFirstValue("sid");

            return Guid.TryParse(sid, out var sessionId) ? sessionId : null;
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            try
            {
                token = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? token["Bearer ".Length..].Trim()
                    : token.Trim();

                TokenValidationParameters parameters = new()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
                    ValidateIssuer = true,
                    ValidIssuer = _settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _settings.Audience,
                    // Allow logout even if the access token is already expired.
                    ValidateLifetime = false
                };

                return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GenerateToken(string userId, string email, string fullName, Guid sessionId)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                throw new NotFoundException(Constants.Exceptions.UserNotFound);
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);

            List<Claim> claims = new()
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.Email, email),
                new(JwtRegisteredClaimNames.Name, fullName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("sid", sessionId.ToString())
            };

            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
