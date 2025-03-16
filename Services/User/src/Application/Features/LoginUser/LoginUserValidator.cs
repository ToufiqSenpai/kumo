using FluentValidation;
using User.Application.Interfaces;
using User.Application.Repositories;

namespace User.Application.Features.LoginUser;

public class LoginUserValidator : AbstractValidator<LoginUserRequestDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    
    public LoginUserValidator(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;

        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100)
            .MustAsync(BeValidEmail);

        RuleFor(x => x.Password)
            .NotNull()
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100)
            .MustAsync(BeValidPassword);
    }
    
    private async Task<bool> BeValidEmail(string email, CancellationToken cancellationToken)
    {
        return !await _userRepository.IsEmailUniqueAsync(email, cancellationToken);
    }

    private async Task<bool> BeValidPassword(
         LoginUserRequestDto dto,
         string password,
         CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        
        return user == null ? false : _passwordHasher.VerifyPassword(password, user.Password);
    }
}