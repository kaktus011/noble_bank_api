using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Application.Features.Auth.Commands.ClearSession;
using NobleBank.Application.Features.Auth.Commands.Login;
using NobleBank.Application.Features.Auth.Commands.Register;

namespace NobleBank.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ITokenService _tokenService;

        public AuthController(IMediator mediator, ITokenService tokenService)
        {
            _mediator = mediator;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return Ok(new { token = result.Token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.HasActiveSession)
                return Conflict(new { hasActiveSession = true });

            if (!result.Success)
                return Unauthorized(new { error = result.Error });

            return Ok(new { token = result.Token });
        }

        /// <summary>
        /// Clears the server-side session for the authenticated user.
        /// Also accepts a raw token in the request body so that browsers can call this
        /// from a beforeunload handler via navigator.sendBeacon (which cannot set headers).
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
        {
            string? userId = null;

            // Normal logout: Authorization header is present
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            // sendBeacon logout: token is in the request body
            else if (request?.Token is not null)
            {
                userId = _tokenService.GetUserIdFromToken(request.Token);
            }

            if (userId is not null)
                await _mediator.Send(new ClearSessionCommand(userId));

            return Ok();
        }
    }

    public record LogoutRequest(string? Token);
}
