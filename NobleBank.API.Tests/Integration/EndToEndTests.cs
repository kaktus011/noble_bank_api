using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using NobleBank.Domain.Common;

namespace NobleBank.API.Tests.Integration
{
    public class EndToEndTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;

        public EndToEndTests(CustomWebAppFactory factory) => _factory = factory;

        [Fact]
        public async Task RequestCard_HappyPath_CreatesCardInDatabase()
        {
            var userId = "user-e2e";
            var sessionId = Guid.NewGuid();

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Test-User", userId);
            client.DefaultRequestHeaders.Add("X-Test-SessionId", sessionId.ToString());

            var command = new
            {
                Type = CardEnum.Type.Credit,
                Brand = CardEnum.Brand.Visa,
                CreditLimit = 1000m
            };

            // Ensure the user exists in the in-memory Identity store with matching SessionId
            using (var seedScope = _factory.Services.CreateScope())
            {
                var db = seedScope.ServiceProvider.GetRequiredService<NobleBank.Infrastructure.Persistence.ApplicationDbContext>();
                var user = db.Users.FirstOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    user = new NobleBank.Domain.Entities.ApplicationUser
                    {
                        Id = userId,
                        UserName = userId,
                        Email = "user-e2e@example.com",
                        FirstName = "E2E",
                        LastName = "User",
                        EmailConfirmed = true,
                        SessionId = sessionId
                    };
                    db.Users.Add(user);
                }
                else
                {
                    user.SessionId = sessionId;
                }

                db.SaveChanges();
            }

            var response = await client.PostAsJsonAsync("/api/Cards/request", command);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var location = response.Headers.Location;
            Assert.NotNull(location);

            // Verify card exists in the in-memory DB
            List<Domain.Entities.Card> cards;
            using (var verifyScope = _factory.Services.CreateScope())
            {
                var dbContext = verifyScope.ServiceProvider.GetRequiredService<NobleBank.Infrastructure.Persistence.ApplicationDbContext>();

                cards = dbContext.Cards.Where(c => c.CreatedBy == userId || c.UserId == userId).ToList();
                if (cards.Count == 0)
                {
                    // In case seed created admin user with same id, search by CreatedBy null-safe
                    cards = dbContext.Cards.Where(c => c.CreatedBy != null && c.CreatedBy.Contains("user-e2e")).ToList();
                }
            }

            Assert.Single(cards);
            var card = cards[0];
            Assert.Equal(CardEnum.Brand.Visa, card.Brand);
            Assert.Equal(CardEnum.Type.Credit, card.CardType);
        }
    }
}
