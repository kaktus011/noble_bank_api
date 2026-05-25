using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NobleBank.Infrastructure.Tests
{

    public class TokenServiceTests
    {
        [Fact]
        public async Task GenerateToken_ShouldCreateJwtWithExpectedClaims()
        {
            // Arrange
            var service = TestHelpers.CreateTokenService();

            // Act
            var token = await service.GenerateToken("user-1", "john.doe@example.com", "John Doe");

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Standard claims
            Assert.Equal("issuer", jwt.Issuer);
            Assert.Contains(jwt.Audiences, a => a == "audience");
            Assert.Equal("user-1", jwt.Claims.First(c => c.Type == "sub").Value);
            Assert.Equal("john.doe@example.com", jwt.Claims.First(c => c.Type == "email").Value);
            Assert.Equal("John Doe", jwt.Claims.First(c => c.Type == "name").Value);
            Assert.NotNull(jwt.Claims.FirstOrDefault(c => c.Type == "jti"));

            // Role claims
            var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            Assert.NotEmpty(roleClaims);
            Assert.Contains(roleClaims, c => c.Value == "User");
        }

        [Fact]
        public async Task GenerateToken_WithMultipleRoles_ShouldIncludeAllRoleClaimsInToken()
        {
            // Arrange
            var service = TestHelpers.CreateTokenServiceWithRoles("Administrator", "User", "Moderator");

            // Act
            var token = await service.GenerateToken("admin-1", "admin@example.com", "Admin User");

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            Assert.NotEmpty(roleClaims);
            Assert.Equal(3, roleClaims.Count);
            Assert.Contains("Administrator", roleClaims);
            Assert.Contains("User", roleClaims);
            Assert.Contains("Moderator", roleClaims);
        }

        [Fact]
        public async Task GenerateToken_WithNoRoles_ShouldNotIncludeRoleClaims()
        {
            // Arrange
            var service = TestHelpers.CreateTokenServiceWithRoles();

            // Act
            var token = await service.GenerateToken("user-no-roles", "user@example.com", "User No Roles");

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            Assert.Empty(roleClaims);
        }
    }
}
