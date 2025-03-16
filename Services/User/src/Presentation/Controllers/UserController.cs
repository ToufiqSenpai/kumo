using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Exceptions;
using User.Application.Features.CreateUser;
using User.Application.Features.GetUserMe;
using User.Application.Features.LoginUser;
using User.Application.Features.LogoutUser;
using User.Application.Features.Refresh;

namespace User.Presentation.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UserController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken)
    {
        return Created(nameof(CreateUser), await mediator.Send(new CreateUserCommand(request), cancellationToken));
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new LoginUserCommand(request), cancellationToken));
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RefreshCommand(request), cancellationToken));
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetMe([FromHeader(Name = "X-User-ID")] string userId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, "Invalid X-User-ID format. Must be a GUID.");
        }
        
        return Ok(await mediator.Send(new GetUserMeQuery(userGuid), cancellationToken));
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutUser([FromBody] LogoutUserRequestDto request, CancellationToken cancellationToken)
    {
        await mediator.Send(new LogoutUserCommand(request), cancellationToken);
        
        return NoContent();
    }
}