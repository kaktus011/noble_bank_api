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
            var sessionId = Guid.NewGuid();

            // Act
            var token = await service.GenerateToken("user-1", "john.doe@example.com", "John Doe", sessionId);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal("issuer", jwt.Issuer);
            Assert.Contains(jwt.Audiences, a => a == "audience");
            Assert.Equal("user-1", jwt.Claims.First(c => c.Type == "sub").Value);
            Assert.Equal("john.doe@example.com", jwt.Claims.First(c => c.Type == "email").Value);
            Assert.Equal("John Doe", jwt.Claims.First(c => c.Type == "name").Value);
            Assert.NotNull(jwt.Claims.FirstOrDefault(c => c.Type == "jti"));
            Assert.Equal(sessionId.ToString(), jwt.Claims.First(c => c.Type == "sid").Value);

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
            var token = await service.GenerateToken("admin-1", "admin@example.com", "Admin User", Guid.NewGuid());

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
        public async Task GetSessionIdFromToken_ShouldReturnSidClaim()
        {
            // Arrange
            var service = TestHelpers.CreateTokenService();
            var sessionId = Guid.NewGuid();
            var token = await service.GenerateToken("user-1", "john.doe@example.com", "John Doe", sessionId);

            // Act
            Guid? extracted = service.GetSessionIdFromToken(token);

            // Assert
            Assert.Equal(sessionId, extracted);
        }

        [Fact]
        public void GetSessionIdFromToken_WithInvalidToken_ShouldReturnNull()
        {
            // Arrange
            var service = TestHelpers.CreateTokenService();

            // Act
            Guid? extracted = service.GetSessionIdFromToken("not-a-jwt");

            // Assert
            Assert.Null(extracted);
        }

        [Fact]
        public async Task GetUserIdFromToken_ShouldReturnSubClaim()
        {
            // Arrange
            var service = TestHelpers.CreateTokenService();
            var token = await service.GenerateToken("user-42", "u@example.com", "U", Guid.NewGuid());

            // Act
            string? extracted = service.GetUserIdFromToken(token);

            // Assert
            Assert.Equal("user-42", extracted);
        }

        [Fact]
        public async Task GetUserIdFromToken_WithBearerPrefix_ShouldStripAndParse()
        {
            // Arrange
            var service = TestHelpers.CreateTokenService();
            var token = await service.GenerateToken("user-42", "u@example.com", "U", Guid.NewGuid());

            // Act
            string? extracted = service.GetUserIdFromToken($"Bearer {token}");

            // Assert
            Assert.Equal("user-42", extracted);
        }

        [Fact]
        public async Task GetUserIdFromToken_WithTamperedSignature_ShouldReturnNull()
        {
            // Arrange
            var service = TestHelpers.CreateTokenService();
            var token = await service.GenerateToken("user-42", "u@example.com", "U", Guid.NewGuid());
            // Flip the first character of the signature segment. The last base64url
            // character of an HMAC-SHA256 signature carries only 4 significant bits
            // (the trailing 2 bits are padding and discarded on decode), so tampering
            // there can produce an identical byte sequence and validate successfully.
            var parts = token.Split('.');
            parts[2] = parts[2].Length > 0
                ? (parts[2][0] == 'A' ? 'B' : 'A') + parts[2][1..]
                : "x";
            var tampered = string.Join('.', parts);

            // Act
            string? extracted = service.GetUserIdFromToken(tampered);

            // Assert
            Assert.Null(extracted);
        }

        [Fact]
        public async Task GenerateToken_WithNoRoles_ShouldNotIncludeRoleClaims()
        {
            // Arrange
            var service = TestHelpers.CreateTokenServiceWithRoles();

            // Act
            var token = await service.GenerateToken("user-no-roles", "user@example.com", "User No Roles", Guid.NewGuid());

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            Assert.Empty(roleClaims);
        }
    }
}
