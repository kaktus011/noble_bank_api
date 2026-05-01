namespace NobleBank.Application.Features.Cards.Queries.GetAllCards
{
    public record CardDto
    {
        public Guid Id { get; init; }
        public string Brand { get; init; } = string.Empty;
        public string Last4Digits { get; init; } = string.Empty;
        public string CardHolder { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public decimal Balance { get; init; }
        public decimal? CreditLimit { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime ExpiryDate { get; init; }
        public bool IsExpired { get; init; }
        public bool IsCredit { get; init; }
    }
}