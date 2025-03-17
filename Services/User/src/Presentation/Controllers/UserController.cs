using System.Net;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Exceptions;
using Shared.Common.Filters;
using User.Application.DTOs;
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
    [ServiceFilter(typeof(ValidationFilter))]
    public async Task<ActionResult<CreateUserResponseDto>> CreateUser([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken)
    {
        return Created(nameof(CreateUser), await mediator.Send(new CreateUserCommand(request), cancellationToken));
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<LoginUserResponseDto>> Login(
        [FromBody] LoginUserRequestDto request, 
        [FromServices] IValidator<LoginUserRequestDto> validator,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        
        if (!result.IsValid)
        {
            throw new HttpResponseException(HttpStatusCode.Unauthorized, "Email or Password is invalid.");
        }
        
        return Ok(await mediator.Send(new LoginUserCommand(request), cancellationToken));
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshResponseDto>> Refresh([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RefreshCommand(request), cancellationToken));
    }
    
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe([FromHeader(Name = "X-User-ID")] string userId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, "Invalid X-User-ID format. Must be a GUID.");
        }
        
        return Ok(await mediator.Send(new GetUserMeQuery(userGuid), cancellationToken));
    }
    
    [HttpPost("logout")]
    public async Task<ActionResult> LogoutUser([FromBody] LogoutUserRequestDto request, CancellationToken cancellationToken)
    {
        await mediator.Send(new LogoutUserCommand(request), cancellationToken);
        
        return NoContent();
    }
}