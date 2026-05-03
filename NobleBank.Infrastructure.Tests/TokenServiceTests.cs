using NobleBank.Infrastructure.Identity;

namespace NobleBank.Infrastructure.Tests
{
    public class TokenServiceTests
    {
        [Fact]
        public void GenerateToken_ShouldCreateJwtWithExpectedClaims()
        {
            // Arrange
            var service = TestHelpers.CreateTokenService();

            // Act
            var token = service.GenerateToken("user-1", "john.doe@example.com", "John Doe");

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal("issuer", jwt.Issuer);
            Assert.Contains(jwt.Audiences, a => a == "audience");
            Assert.Equal("user-1", jwt.Claims.First(c => c.Type == "sub").Value);
            Assert.Equal("john.doe@example.com", jwt.Claims.First(c => c.Type == "email").Value);
            Assert.Equal("John Doe", jwt.Claims.First(c => c.Type == "name").Value);
            Assert.NotNull(jwt.Claims.FirstOrDefault(c => c.Type == "jti"));
        }
    }
}
