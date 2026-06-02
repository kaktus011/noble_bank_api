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
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                return jwt.Subject; // the 'sub' claim
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
