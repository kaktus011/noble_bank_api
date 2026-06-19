using Microsoft.AspNetCore.Mvc;
using NobleBank.API.Controllers;
using NobleBank.Application.Features.Loans.Queries.GetLoanOptions;
using NobleBank.Domain.Common;

namespace NobleBank.API.Tests
{
    public class LoansControllerTests
    {
        [Fact]
        public void GetOptions_ShouldReturnAllLoanTypes()
        {
            // Arrange
            var controller = CreateController(userId: "user-1");

            // Act
            ActionResult<LoanOptionsDto> result = controller.GetOptions();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var options = Assert.IsType<LoanOptionsDto>(ok.Value);

            var expectedTypes = Enum.GetValues<LoansEnum.Type>();
            Assert.Equal(expectedTypes.Length, options.Types.Count);
            foreach (var t in expectedTypes)
            {
                Assert.Contains(options.Types, o => o.Value == (int)t && o.Name == t.ToString());
            }
        }

        private static LoansController CreateController(string? userId, Func<object, object?>? mediatorHandler = null)
        {
            var controller = new LoansController(new TestHelpers.RecordingMediator(mediatorHandler));
            controller.ControllerContext = TestHelpers.CreateControllerContext(userId);
            return controller;
        }
    }
}
