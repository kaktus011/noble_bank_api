namespace NobleBank.Application.Features.Auth
{
    public record AuthResult(
        bool Success,
        string? Token,
        string? Error);
}
