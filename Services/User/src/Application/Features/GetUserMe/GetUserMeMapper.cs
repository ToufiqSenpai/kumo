using AutoMapper;
using User.Application.DTOs;

namespace User.Application.Features.GetUserMe;

public class GetUserMeMapper : Profile
{
    public GetUserMeMapper()
    {
        CreateMap<Domain.Entities.User, UserDto>();
    }
}