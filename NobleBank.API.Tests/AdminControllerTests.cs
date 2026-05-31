using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobleBank.API.Controllers;
using NobleBank.Domain.Common;
using NobleBank.Application.Features.Admin.Queries.GetPendingCards;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Application.Features.Cards.Queries.GetCardById;
using NobleBank.Application.Features.Admin.Queries.GetPendingLoans;
using NobleBank.Application.Features.Loans.Queries.GetAllLoans;
using NobleBank.Application.Features.Loans.Queries.GetLoanById;
using NobleBank.Application.Features.Transactions.Queries.GetAllTransactions;
using NobleBank.Application.Features.Transactions.Queries.GetTransactionById;

namespace NobleBank.API.Tests
{
    public class AdminControllerTests
    {
        [Fact]
        public void AdminController_HasAuthorizeAttribute_WithAdministratorRole()
        {
            var attr = (AuthorizeAttribute?)typeof(AdminController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .FirstOrDefault();

            Assert.NotNull(attr);
            Assert.Equal(Roles.Administrator, attr!.Roles);
        }

        // ===== CARDS =====

        [Fact]
        public async Task GetAllCards_WhenCalled_SendsGetAllCardsQuery()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator();
            var controller = new AdminController(mediator);

            // Act
            await controller.GetAllCards();

            // Assert
            Assert.Single(mediator.Requests);
            var req = mediator.Requests[0];
            Assert.IsType<GetAllCardsQuery>(req);
            var query = (GetAllCardsQuery)req;
            Assert.Null(query.UserId);
        }

        [Fact]
        public async Task GetCardById_WhenCardExists_SendsGetCardByIdQueryAndReturnsOk()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator();
            var cardId = Guid.NewGuid();
            mediator.Response = new CardDto { Id = cardId };
            var controller = new AdminController(mediator);

            // Act
            var result = await controller.GetCardById(cardId);

            // Assert
            Assert.Single(mediator.Requests);
            var req = mediator.Requests[0];
            Assert.IsType<GetCardByIdQuery>(req);
            var query = (GetCardByIdQuery)req;
            Assert.Equal(cardId, query.CardId);
            Assert.Null(query.UserId);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCardById_WhenCardDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator();
            mediator.Response = null;
            var controller = new AdminController(mediator);
            var cardId = Guid.NewGuid();

            // Act
            var result = await controller.GetCardById(cardId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPendingCards_WhenCalled_SendsGetPendingCardsQuery()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator();
            var controller = new AdminController(mediator);

            // Act
            await controller.GetPendingCards();

            // Assert
            Assert.Single(mediator.Requests);
            Assert.IsType<GetPendingCardsQuery>(mediator.Requests[0]);
        }

        [Fact]
        public async Task ApproveCard_WhenCalled_ByAdmin_SendsApproveCardCommand()
        {
            // Arrange
            string userId = "admin-1";
            var mediator = new TestHelpers.RecordingMediator();
            var controller = new AdminController(mediator);
            controller.ControllerContext = CreateControllerContextWithRole(userId, Roles.Administrator);

            var cardId = Guid.NewGuid();

            // Act
            await controller.ApproveCard(cardId);

            // Assert
            Assert.Single(mediator.Requests);
            var req = mediator.Requests[0];
            Assert.IsType<NobleBank.Application.Features.Admin.Commands.ApproveCard.ApproveCardCommand>(req);
            var cmd = (NobleBank.Application.Features.Admin.Commands.ApproveCard.ApproveCardCommand)req;
            Assert.Equal(userId, cmd.AdminUserId);
            Assert.Equal(cardId, cmd.CardId);
        }

        [Fact]
        public async Task RejectCard_WhenCalled_ByAdmin_SendsRejectCardCommand()
        {
            // Arrange
            string userId = "admin-1";
            var mediator = new TestHelpers.RecordingMediator();
            var controller = new AdminController(mediator);
            controller.ControllerContext = CreateControllerContextWithRole(userId, Roles.Administrator);

            var cardId = Guid.NewGuid();
            var request = new RejectRequest("Not eligible");

            // Act
            await controller.RejectCard(cardId, request);

            // Assert
            Assert.Single(mediator.Requests);
            var req = mediator.Requests[0];
            Assert.IsType<NobleBank.Application.Features.Admin.Commands.RejectCard.RejectCardCommand>(req);
            var cmd = (NobleBank.Application.Features.Admin.Commands.RejectCard.RejectCardCommand)req;
            Assert.Equal(userId, cmd.AdminUserId);
            Assert.Equal(cardId, cmd.CardId);
            Assert.Equal(request.Reason, cmd.Reason);
        }

        // ===== LOANS =====

        [Fact]
        public async Task GetAllLoans_WhenCalled_SendsGetAllLoansQuery()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator();
            var controller = new AdminController(mediator);

            // Act
            await controller.GetAllLoans();

            // Assert
            Assert.Single(mediator.Requests);
            var req = mediator.Requests[0];
            Assert.IsType<GetAllLoansQuery>(req);
            var query = (GetAllLoansQuery)req;
            Assert.Null(query.UserId);
        }

        [Fact]
        public async Task GetLoanById_WhenLoanExists_SendsGetLoanByIdQueryAndReturnsOk()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator();
            var loanId = Guid.NewGuid();
            mediator.Response = new LoanDto { Id = loanId };
            var controller = new AdminController(mediator);

            // Act
            var result = await controller.GetLoanById(loanId);

            // Assert
            Assert.Single(mediator.Requests);
            var req = mediator.Requests[0];
            Assert.IsType<GetLoanByIdQuery>(req);
            var query = (GetLoanByIdQuery)req;
            Assert.Equal(loanId, query.LoanId);
            Assert.Null(query.UserId);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetLoanById_WhenLoanDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator();
            mediator.Response = null;
            var controller = new AdminController(mediator);
            var loanId = Guid.NewGuid();

            // Act
            var result = await controller.GetLoanById(loanId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPendingLoans_WhenCalled_SendsGetPendingLoansQuery()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator();
            var controller = new AdminController(mediator);

            // Act
            await controller.GetPendingLoans();

            // Assert
            Assert.Single(mediator.Requests);
            Assert.IsType<GetPendingLoansQuery>(mediator.Requests[0]);
        }

        [Fact]
        public async Task ApproveLoan_WhenCalled_ByAdmin_SendsApproveLoanCommand()
        {
            // Arrange
            string userId = "admin-1";
            var mediator = new TestHelpers.RecordingMediator();
            var controller = new AdminController(mediator);
            controller.ControllerContext = CreateControllerContextWithRole(userId, Roles.Administrator);

            var loanId = Guid.NewGuid();

            // Act
            await controller.ApproveLoan(loanId);

            // Assert
            Assert.Single(mediator.Requests);
            var req = mediator.Requests[0];
            Assert.IsType<NobleBank.Application.Features.Admin.Commands.ApproveLoan.ApproveLoanCommand>(req);
            var cmd = (NobleBank.Application.Features.Admin.Commands.ApproveLoan.ApproveLoanCommand)req;
            Assert.Equal(userId, cmd.AdminUserId);
            Assert.Equal(loanId, cmd.LoanId);
        }

        [Fact]
        public async Task RejectLoan_WhenCalled_ByAdmin_SendsRejectLoanCommand()
        {
            // Arrange
            string userId = "admin-1";
            var mediator = new TestHelpers.RecordingMediator();
            var controller = new AdminController(mediator);
            controller.ControllerContext = CreateControllerContextWithRole(userId, Roles.Administrator);

            var loanId = Guid.NewGuid();
            var request = new RejectRequest("Bad credit history");

            // Act
            await controller.RejectLoan(loanId, request);

            // Assert
            Assert.Single(mediator.Requests);
            var req = mediator.Requests[0];
            Assert.IsType<NobleBank.Application.Features.Admin.Commands.RejectLoan.RejectLoanCommand>(req);
            var cmd = (NobleBank.Application.Features.Admin.Commands.RejectLoan.RejectLoanCommand)req;
            Assert.Equal(userId, cmd.AdminUserId);
            Assert.Equal(loanId, cmd.LoanId);
            Assert.Equal(request.Reason, cmd.Reason);
        }

        // ===== TRANSACTIONS =====

        [Fact]
        public async Task GetAllTransactions_WhenCalled_SendsGetAllTransactionsQuery()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator();
            var controller = new AdminController(mediator);

            // Act
            await controller.GetAllTransactions();

            // Assert
            Assert.Single(mediator.Requests);
            var req = mediator.Requests[0];
            Assert.IsType<GetAllTransactionsQuery>(req);
            var query = (GetAllTransactionsQuery)req;
            Assert.Null(query.UserId);
            Assert.Null(query.CardId);
            Assert.Equal(50, query.Limit);
        }

        [Fact]
        public async Task GetTransactionById_WhenTransactionExists_SendsGetTransactionByIdQueryAndReturnsOk()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator();
            var transactionId = Guid.NewGuid();
            mediator.Response = new TransactionDto { Id = transactionId };
            var controller = new AdminController(mediator);

            // Act
            var result = await controller.GetTransactionById(transactionId);

            // Assert
            Assert.Single(mediator.Requests);
            var req = mediator.Requests[0];
            Assert.IsType<GetTransactionByIdQuery>(req);
            var query = (GetTransactionByIdQuery)req;
            Assert.Equal(transactionId, query.TransactionId);
            Assert.Null(query.UserId);
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetTransactionById_WhenTransactionDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var mediator = new TestHelpers.RecordingMediator();
            mediator.Response = null;
            var controller = new AdminController(mediator);
            var transactionId = Guid.NewGuid();

            // Act
            var result = await controller.GetTransactionById(transactionId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        private static ControllerContext CreateControllerContextWithRole(string userId, string role)
        {
            var ctx = new DefaultHttpContext();
            ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            }, "TestAuth"));

            return new ControllerContext { HttpContext = ctx };
        }
    }
}
