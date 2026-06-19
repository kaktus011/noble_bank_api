namespace NobleBank.Application.Features.Cards.Queries.GetCardOptions
{
    public record CardEnumOption(int Value, string Name);

    public record CardOptionsDto(
        IReadOnlyList<CardEnumOption> Types,
        IReadOnlyList<CardEnumOption> Brands);
}
