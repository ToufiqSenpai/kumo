using AutoMapper;

namespace User.Application.Features.CreateUser;

public class CreateUserMapper : Profile
{
    public CreateUserMapper()
    {
        CreateMap<CreateUserRequestDto, Domain.Entities.User>();
    }
}