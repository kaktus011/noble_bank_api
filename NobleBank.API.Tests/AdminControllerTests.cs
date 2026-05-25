using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobleBank.API.Controllers;
using NobleBank.Domain.Common;
using Xunit;

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
