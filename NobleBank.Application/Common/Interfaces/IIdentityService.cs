namespace NobleBank.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<(bool Success, string UserId, string Error)> RegisterAsync(
            string email,
            string password,
            string firstName,
            string lastName);

        Task<(bool Success, string UserId, string Error)> LoginAsync(
            string email,
            string password);
    }
}
