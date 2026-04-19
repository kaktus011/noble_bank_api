using AutoMapper;
using MediatR;
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
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

            if (user is null)
                throw new Exception("User not found");

            // Генерира картов номер (Luhn-валиден)
            var cardNumber = GenerateCardNumber(request.Brand);

            var card = Card.Create(
                cardHolder: user.FullName,
                plainCardNumber: cardNumber,
                type: request.Type,
                brand: request.Brand,
                userId: request.UserId,
                createdBy: request.UserId,
                initialBalance: 0,
                creditLimit: request.CreditLimit
            );

            // Автоматично активираме (в реалност би чакало одобрение)
            card.Activate();

            _context.Cards.Add(card);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CardDto>(card);
        }

        private string GenerateCardNumber(Enum brand)
        {
            var prefix = brand switch
            {
                CardEnum.Brand.Visa => "4",
                CardEnum.Brand.Mastercard => "5",
                CardEnum.Brand.AmericanExpress => "3",
                CardEnum.Brand.Maestro => "6",
                _ => "4"
            };

            // Генерира 15 случайни цифри + добавя Luhn check digit
            var random = new Random();
            var number = prefix + string.Join("", Enumerable.Range(0, 14).Select(_ => random.Next(0, 10)));

            return number + CalculateLuhnCheckDigit(number);
        }

        private int CalculateLuhnCheckDigit(string number)
        {
            var sum = 0;
            var isSecond = false;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                var digit = number[i] - '0';

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
