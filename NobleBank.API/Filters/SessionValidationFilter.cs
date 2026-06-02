using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;

namespace NobleBank.API.Filters
{
    public class SessionValidationFilter : IAsyncAuthorizationFilter
    {
        private readonly IApplicationDbContext _context;

        public SessionValidationFilter(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
                return;

            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionIdClaim = context.HttpContext.User.FindFirstValue("sid");

            if (userId is null || !Guid.TryParse(sessionIdClaim, out var tokenSessionId))
                return;

            var dbSessionId = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => (Guid?)u.SessionId)
                .FirstOrDefaultAsync();

            if (dbSessionId != tokenSessionId)
                context.Result = new UnauthorizedObjectResult(new { error = "Session has been terminated." });
        }
    }
}
