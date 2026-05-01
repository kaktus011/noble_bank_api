using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Exceptions;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Cards.Queries.GetAllCards;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

namespace NobleBank.Application.Features.Cards.Commands.RequestCard
{
    public class RequestCardCommandHandler : IRequestHandler<RequestCardCommand, CardDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RequestCardCommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CardDto> Handle(RequestCardCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                throw new UnauthorizedAccessException("User ID is required.");
            }

            ApplicationUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                throw new NotFoundException($"User '{request.UserId}' was not found.");
            }

            string cardNumber = GenerateCardNumber(request.Brand);

            Card card = Card.Create(
                cardHolder: user.FullName,
                plainCardNumber: cardNumber,
                type: request.Type,
                brand: request.Brand,
                userId: request.UserId,
                createdBy: request.UserId,
                initialBalance: 0,
                creditLimit: request.CreditLimit
            );

            card.Activate();

            _context.Cards.Add(card);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CardDto>(card);
        }

        private string GenerateCardNumber(CardEnum.Brand brand)
        {
            string prefix = brand switch
            {
                CardEnum.Brand.Visa => "4",
                CardEnum.Brand.Mastercard => "5",
                CardEnum.Brand.AmericanExpress => "3",
                CardEnum.Brand.Maestro => "6",
                _ => "4"
            };

            string randomDigits = string.Concat(
                Enumerable.Range(0, 14)
                    .Select(_ => System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 10).ToString()));
            string number = prefix + randomDigits;

            return number + CalculateLuhnCheckDigit(number);
        }

        private int CalculateLuhnCheckDigit(string number)
        {
            int sum = 0;
            bool isSecond = false;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                int digit = number[i] - '0';

                if (isSecond)
                {
                    digit *= 2;
                    if (digit > 9) digit -= 9;
                }

                sum += digit;
                isSecond = !isSecond;
            }

            return (10 - (sum % 10)) % 10;
        }
    }
}
