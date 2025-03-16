using System.Net;
using AutoMapper;
using MediatR;
using Shared.Common.Exceptions;
using User.Application.DTOs;
using User.Application.Repositories;

namespace User.Application.Features.GetUserMe;

public sealed class GetUserMeHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetUserMeQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserMeQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound, "User not found.");
        }
        
        return mapper.Map<UserDto>(user);
    }
}