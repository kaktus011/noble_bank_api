using MediatR;
using NobleBank.Application.Features.Auth;
using NobleBank.Application.Features.Auth.Commands.Login;
using NobleBank.Application.Features.Auth.Commands.Register;
using NobleBank.Application.Features.Cards.Commands.RequestCard;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Application.Features.Cards.Queries.GetCardById;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;
using NobleBank.API.Controllers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace NobleBank.API.Tests
{
    internal static class TestHelpers
    {
        public static ControllerContext CreateControllerContext(string? userId = "user-1")
        {
            var httpContext = new DefaultHttpContext();
            if (userId is not null)
            {
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, userId)
                ], "TestAuth"));
            }
            else
            {
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            }

            return new ControllerContext { HttpContext = httpContext };
        }

        public static CardDto CreateCardDto(Guid? id = null, string? cardHolder = null, string? brand = null, string? type = null, string? status = null, string? last4 = null)
            => new()
            {
                Id = id ?? Guid.NewGuid(),
                CardHolder = cardHolder ?? "JOHN DOE",
                Brand = brand ?? "Visa",
                Type = type ?? "Credit",
                Status = status ?? "Active",
                Last4Digits = last4 ?? "4242",
                Balance = 100m,
                CreditLimit = 500m,
                Currency = Constants.Card.DefaultCurrency,
                ExpiryDate = DateTime.UtcNow.AddYears(2),
                IsCredit = true,
                IsExpired = false
            };

        internal sealed class RecordingMediator : IMediator
        {
            private readonly Func<object, object?> _handler;

            public RecordingMediator(Func<object, object?>? handler = null)
            {
                _handler = handler ?? (_ => null);
            }

            public List<object> Requests { get; } = [];

            public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;
            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                Requests.Add(request!);
                var response = _handler(request!);
                return Task.FromResult((TResponse?)response!);
            }
            public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            {
                Requests.Add(request);
                return Task.FromResult(_handler(request));
            }
            public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotImplementedException();
            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }
    }
}
