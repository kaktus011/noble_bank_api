namespace NobleBank.Application.Common.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateToken(string userId, string email, string fullName, Guid sessionId);

        string? GetUserIdFromToken(string token);
    }
}
