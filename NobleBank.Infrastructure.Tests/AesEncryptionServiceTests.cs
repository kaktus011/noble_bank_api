using Microsoft.Extensions.Options;
using NobleBank.Infrastructure.Services;
using NobleBank.Infrastructure.Settings;

namespace NobleBank.Infrastructure.Tests
{
    public class AesEncryptionServiceTests
    {
        [Fact]
        public void Encrypt_And_Decrypt_ShouldRoundTrip()
        {
            // Arrange
            var service = TestHelpers.CreateEncryptionService();
            var plainText = "Hello, world!";

            // Act
            var encrypted = service.Encrypt(plainText);
            var decrypted = service.Decrypt(encrypted);

            // Assert
            Assert.NotEqual(plainText, encrypted);
            Assert.Equal(plainText, decrypted);
        }
    }
}
