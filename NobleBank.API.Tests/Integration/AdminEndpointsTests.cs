using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace NobleBank.API.Tests.Integration
{
    public class AdminEndpointsTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;

        public AdminEndpointsTests(CustomWebAppFactory factory) => _factory = factory;

        [Fact]
        public async Task ApproveCard_AdminRole_ReturnsNoContent()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Test-User", "admin-1");
            client.DefaultRequestHeaders.Add("X-Test-Roles", "Administrator");

            var id = Guid.NewGuid();

            // Ensure a card exists in the test DB with Pending status
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<NobleBank.Infrastructure.Persistence.ApplicationDbContext>();

                // Create an owner user for the card
                var ownerId = "owner-1";
                var owner = new NobleBank.Domain.Entities.ApplicationUser { Id = ownerId, UserName = "owner@test", Email = "owner@test" };
                // Try add owner only if not exists
                if (!db.Users.Any(u => u.Id == ownerId))
                {
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
                card.Id = id;

                db.Cards.Add(card);
                db.SaveChanges();
            }

            var response = await client.PostAsync($"/api/Admin/cards/{id}/approve", null);

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
