using NobleBank.API.Controllers;
using NobleBank.Application.Features.Transactions.Commands.CreateTransaction;
using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using NobleBank.Application.Features.Transactions.Queries.GetTransactionById;
using Microsoft.AspNetCore.Mvc;
using NobleBank.Domain.Common;

namespace NobleBank.API.Tests
{
    public class TransactionsControllerTests
    {
        private TransactionsController CreateController(string? userId = "user-1")
        {
            var mediator = new TestHelpers.RecordingMediator(request =>
            {
                return request switch
                {
                    GetAllTransactionsQuery => new List<TransactionDto>(),
                    GetTransactionByIdQuery => null,
                    CreateTransactionCommand cmd => new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        Amount = cmd.Amount,
                        Description = cmd.Description,
                        Type = cmd.Type.ToString(),
                        OccurredAt = DateTime.UtcNow,
                        CardId = cmd.CardId,
                        CardLast4 = "4242"
                    },
                    _ => null
                };
            });

            var controller = new TransactionsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(userId)
            };
            return controller;
        }

        [Fact]
        public async Task GetAll_WhenUserIdExists_ShouldReturnOkWithTransactions()
        {
            // Arrange
            var userId = "user-1";
            var mediator = new TestHelpers.RecordingMediator(request =>
            {
                return new List<TransactionDto>
                {
                    new() { Id = Guid.NewGuid(), Amount = 100m, Description = "Trans 1", Type = "Expense", OccurredAt = DateTime.UtcNow, CardId = Guid.NewGuid(), CardLast4 = "4242" },
                    new() { Id = Guid.NewGuid(), Amount = 200m, Description = "Trans 2", Type = "Income", OccurredAt = DateTime.UtcNow, CardId = Guid.NewGuid(), CardLast4 = "4242" }
                };
            });

            var controller = new TransactionsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(userId)
            };

            // Act
            var result = await controller.GetAll(null, 50);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransactions = Assert.IsAssignableFrom<List<TransactionDto>>(okResult.Value);
            Assert.Equal(2, returnedTransactions.Count);
        }

        [Fact]
        public async Task GetAll_WithCardIdFilter_ShouldPassCardIdToQuery()
        {
            // Arrange
            var userId = "user-1";
            var cardId = Guid.NewGuid();
            var mediator = new TestHelpers.RecordingMediator(_ => new List<TransactionDto>());

            var controller = new TransactionsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(userId)
            };

            // Act
            await controller.GetAll(cardId, 50);

            // Assert
            var request = Assert.Single(mediator.Requests);
            var query = Assert.IsType<GetAllTransactionsQuery>(request);
            Assert.Equal(cardId, query.CardId);
        }

        [Fact]
        public async Task GetAll_WithLimitParameter_ShouldPassLimitToQuery()
        {
            // Arrange
            var userId = "user-1";
            var limit = 25;
            var mediator = new TestHelpers.RecordingMediator(_ => new List<TransactionDto>());

            var controller = new TransactionsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(userId)
            };

            // Act
            await controller.GetAll(null, limit);

            // Assert
            var request = Assert.Single(mediator.Requests);
            var query = Assert.IsType<GetAllTransactionsQuery>(request);
            Assert.Equal(limit, query.Limit);
        }

        [Fact]
        public async Task GetAll_WhenUserIdIsMissing_ShouldReturnUnauthorized()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator(_ => new List<TransactionDto>());
            var controller = new TransactionsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(null)
            };

            // Act
            var result = await controller.GetAll(null, 50);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task GetAll_ShouldReturnEmptyListWhenNoTransactions()
        {
            // Arrange
            var userId = "user-1";
            var mediator = new TestHelpers.RecordingMediator(_ => new List<TransactionDto>());

            var controller = new TransactionsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(userId)
            };

            // Act
            var result = await controller.GetAll(null, 50);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransactions = Assert.IsAssignableFrom<List<TransactionDto>>(okResult.Value);
            Assert.Empty(returnedTransactions);
        }

        [Fact]
        public async Task GetById_WhenTransactionExists_ShouldReturnOkWithTransaction()
        {
            // Arrange
            var userId = "user-1";
            var transactionId = Guid.NewGuid();
            var transaction = new TransactionDto
            {
                Id = transactionId,
                Amount = 100m,
                Description = "Test",
                Type = "Expense",
                OccurredAt = DateTime.UtcNow,
                CardId = Guid.NewGuid(),
                CardLast4 = "4242"
            };

            var mediator = new TestHelpers.RecordingMediator(_ => transaction);

            var controller = new TransactionsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(userId)
            };

            // Act
            var result = await controller.GetById(transactionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransaction = Assert.IsType<TransactionDto>(okResult.Value);
            Assert.Equal(transactionId, returnedTransaction.Id);
        }

        [Fact]
        public async Task GetById_WhenTransactionDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var userId = "user-1";
            var transactionId = Guid.NewGuid();
            var mediator = new TestHelpers.RecordingMediator(_ => null);

            var controller = new TransactionsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(userId)
            };

            // Act
            var result = await controller.GetById(transactionId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task GetById_WhenUserIdIsMissing_ShouldReturnUnauthorized()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var mediator = new TestHelpers.RecordingMediator(_ => null);

            var controller = new TransactionsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(null)
            };

            // Act
            var result = await controller.GetById(transactionId);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task Create_WithValidCommand_ShouldReturnCreatedAtActionWithTransaction()
        {
            // Arrange
            var userId = "user-1";
            var transactionId = Guid.NewGuid();
            var cardId = Guid.NewGuid();
            var transaction = new TransactionDto
            {
                Id = transactionId,
                Amount = 100m,
                Description = "Test",
                Type = "Expense",
                OccurredAt = DateTime.UtcNow,
                CardId = cardId,
                CardLast4 = "4242"
            };

            var mediator = new TestHelpers.RecordingMediator(_ => transaction);

            var controller = new TransactionsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(userId)
            };

            var command = new CreateTransactionCommand(
                UserId: userId,
                CardId: cardId,
                Amount: 100m,
                Description: "Test",
                Type: TransactionsEnum.Type.Expense);

            // Act
            var result = await controller.Create(command);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(TransactionsController.GetById), createdResult.ActionName);
            var returnedTransaction = Assert.IsType<TransactionDto>(createdResult.Value);
            Assert.Equal(transactionId, returnedTransaction.Id);
        }

        [Fact]
        public async Task Create_ShouldInjectUserIdIntoCommand()
        {
            // Arrange
            var userId = "user-1";
            var cardId = Guid.NewGuid();
            var mediator = new TestHelpers.RecordingMediator(request =>
            {
                if (request is CreateTransactionCommand cmd)
                {
                    return new TransactionDto
                    {
                        Id = Guid.NewGuid(),
                        Amount = cmd.Amount,
                        Description = cmd.Description,
                        Type = cmd.Type.ToString(),
                        OccurredAt = DateTime.UtcNow,
                        CardId = cmd.CardId,
                        CardLast4 = "4242"
                    };
                }
                return null;
            });

            var controller = new TransactionsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(userId)
            };

            var command = new CreateTransactionCommand(
                UserId: null,
                CardId: cardId,
                Amount: 100m,
                Description: "Test",
                Type: TransactionsEnum.Type.Expense);

            // Act
            await controller.Create(command);

            // Assert
            var request = Assert.Single(mediator.Requests);
            var sentCommand = Assert.IsType<CreateTransactionCommand>(request);
            Assert.Equal(userId, sentCommand.UserId);
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedAtActionWithCorrectRoute()
        {
            // Arrange
            var userId = "user-1";
            var transactionId = Guid.NewGuid();
            var cardId = Guid.NewGuid();
            var transaction = new TransactionDto
            {
                Id = transactionId,
                Amount = 100m,
                Description = "Test",
                Type = "Expense",
                OccurredAt = DateTime.UtcNow,
                CardId = cardId,
                CardLast4 = "4242"
            };

            var mediator = new TestHelpers.RecordingMediator(_ => transaction);

            var controller = new TransactionsController(mediator)
            {
                ControllerContext = TestHelpers.CreateControllerContext(userId)
            };

            var command = new CreateTransactionCommand(
                UserId: userId,
                CardId: cardId,
                Amount: 100m,
                Description: "Test",
                Type: TransactionsEnum.Type.Expense);

            // Act
            var result = await controller.Create(command);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(TransactionsController.GetById), createdResult.ActionName);
            Assert.Equal(transactionId, ((TransactionDto)createdResult.Value!).Id);
        }
    }
}
