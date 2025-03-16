using AutoMapper;
using MassTransit;
using MediatR;
using User.Application.Interfaces;
using User.Application.Repositories;
using Shared.Events.Item;

namespace User.Application.Features.CreateUser;

public sealed class CreateUserHandler(
    IUserRepository userRepository, 
    IPasswordHasher passwordHasher, 
    IMapper mapper,
    IPublishEndpoint publishEndpoint) : IRequestHandler<CreateUserCommand, CreateUserResponseDto>
{
    public async Task<CreateUserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = mapper.Map<Domain.Entities.User>(request.Dto);
        user.Password = passwordHasher.HashPassword(user.Password);
        
        await userRepository.AddAsync(user, cancellationToken);
        await publishEndpoint.Publish(new CreateRootFolder(user.Id), cancellationToken);
        
        return new CreateUserResponseDto("User has been created successfully.");
    }
}