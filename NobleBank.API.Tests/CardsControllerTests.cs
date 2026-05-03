using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NobleBank.API.Controllers;
using NobleBank.Application.Features.Cards.Commands.RequestCard;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Application.Features.Cards.Queries.GetCardById;

namespace NobleBank.API.Tests
{
    public class CardsControllerTests
    {
        [Fact]
        public async Task GetAll_WhenUserIdIsMissing_ShouldReturnUnauthorized()
        {
            // Arrange
            var controller = CreateController(null);

            // Act
            ActionResult<List<CardDto>> result = await controller.GetAll();

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task GetAll_WhenUserIdExists_ShouldReturnOkWithCards()
        {
            // Arrange
            var cards = new List<CardDto>
            {
                TestHelpers.CreateCardDto(last4: "1111"),
                TestHelpers.CreateCardDto(last4: "2222")
            };
            var controller = CreateController(userId: "user-1", mediatorHandler: request => request switch
            {
                GetAllCardsQuery => cards,
                _ => null
            });

            // Act
            ActionResult<List<CardDto>> result = await controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(cards, ok.Value);
        }

        [Fact]
        public async Task GetById_WhenCardNotFound_ShouldReturnProblem404()
        {
            // Arrange
            var controller = CreateController(userId: "user-1", mediatorHandler: request => request switch
            {
                GetCardByIdQuery => null,
                _ => null
            });

            // Act
            ActionResult<CardDto> result = await controller.GetById(Guid.NewGuid());

            // Assert
            var problem = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
        }

        [Fact]
        public async Task GetById_WhenCardExists_ShouldReturnOk()
        {
            // Arrange
            var dto = TestHelpers.CreateCardDto();
            var controller = CreateController(userId: "user-1", mediatorHandler: request => request switch
            {
                GetCardByIdQuery => dto,
                _ => null
            });

            // Act
            ActionResult<CardDto> result = await controller.GetById(Guid.NewGuid());

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(dto, ok.Value);
        }

        [Fact]
        public async Task RequestCard_WhenUserIdIsMissing_ShouldReturnUnauthorized()
        {
            // Arrange
            var controller = CreateController(null);

            // Act
            ActionResult<CardDto> result = await controller.RequestCard(new RequestCardCommand(null, NobleBank.Domain.Common.CardEnum.Type.Credit, NobleBank.Domain.Common.CardEnum.Brand.Visa, 250m));

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task RequestCard_WhenUserIdExists_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var dto = TestHelpers.CreateCardDto();
            var controller = CreateController(userId: "user-1", mediatorHandler: request => request switch
            {
                RequestCardCommand => dto,
                _ => null
            });
            var command = new RequestCardCommand(null, NobleBank.Domain.Common.CardEnum.Type.Credit, NobleBank.Domain.Common.CardEnum.Brand.Visa, 250m);

            // Act
            ActionResult<CardDto> result = await controller.RequestCard(command);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(CardsController.GetById), created.ActionName);
            Assert.Same(dto, created.Value);
        }

        private static CardsController CreateController(string? userId, Func<object, object?>? mediatorHandler = null)
        {
            var controller = new CardsController(new TestHelpers.RecordingMediator(mediatorHandler));
            controller.ControllerContext = TestHelpers.CreateControllerContext(userId);
            return controller;
        }
    }
}