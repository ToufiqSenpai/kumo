using Shared.Common.DTOs;

namespace User.Application.Features.CreateUser;

public record CreateUserResponseDto(string Message) : CommonResponseDto(Message);