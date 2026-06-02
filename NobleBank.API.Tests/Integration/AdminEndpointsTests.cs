using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace NobleBank.API.Tests.Integration
{
    public class AdminEndpointsTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;

        public AdminEndpointsTests(CustomWebAppFactory factory) => _factory = factory;

        [Fact]
        public async Task ApproveCard_AdminRole_ReturnsNoContent()
        {
            var adminUserId = "admin-1";
            var sessionId = Guid.NewGuid();

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Test-User", adminUserId);
            client.DefaultRequestHeaders.Add("X-Test-Roles", "Administrator");
            client.DefaultRequestHeaders.Add("X-Test-SessionId", sessionId.ToString());

            var cardId = Guid.NewGuid();

            // Ensure admin user and card exist in the test DB
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<NobleBank.Infrastructure.Persistence.ApplicationDbContext>();

                // Create or update admin user with matching session ID
                var admin = db.Users.FirstOrDefault(u => u.Id == adminUserId);
                if (admin == null)
                {
                    admin = new NobleBank.Domain.Entities.ApplicationUser 
                    { 
                        Id = adminUserId, 
                        UserName = "admin@test", 
                        Email = "admin@test",
                        SessionId = sessionId
                    };
                    db.Users.Add(admin);
                }
                else
                {
                    admin.SessionId = sessionId;
                }

                // Create an owner user for the card
                var ownerId = "owner-1";
                var owner = db.Users.FirstOrDefault(u => u.Id == ownerId);
                if (owner == null)
                {
                    owner = new NobleBank.Domain.Entities.ApplicationUser 
                    { 
                        Id = ownerId, 
                        UserName = "owner@test", 
                        Email = "owner@test",
                        SessionId = Guid.NewGuid()
                    };
                    db.Users.Add(owner);
                }

                var card = NobleBank.Domain.Entities.Card.Create(
                    cardHolder: "Owner",
                    plainCardNumber: "4000000000000000",
                    type: NobleBank.Domain.Common.CardEnum.Type.Credit,
                    brand: NobleBank.Domain.Common.CardEnum.Brand.Visa,
                    userId: ownerId,
                    createdBy: ownerId,
                    initialBalance: 0,
                    creditLimit: 100m);

                // Ensure Id matches the one we'll call
                card.Id = cardId;

                db.Cards.Add(card);
                db.SaveChanges();
            }

            var response = await client.PostAsync($"/api/Admin/cards/{cardId}/approve", null);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task ApproveCard_NonAdminRole_ReturnsForbidden()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Test-User", "user-1");

            var id = Guid.NewGuid();
            var response = await client.PostAsync($"/api/Admin/cards/{id}/approve", null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
