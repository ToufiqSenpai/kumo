using FluentValidation;
using User.Application.Repositories;

namespace User.Application.Features.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserRequestDto>
{
    private readonly IUserRepository _userRepository;
    
    public CreateUserValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        
        RuleFor(x => x.Name)
            .NotNull().WithMessage("Name is required.")
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(1).WithMessage("Name must be at least 1 character.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        
        RuleFor(x => x.Email)
            .NotNull().WithMessage("Email is required.")
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.")
            .EmailAddress().WithMessage("Invalid email address.")
            .MustAsync(IsEmailUnique).WithMessage("Email is already in use.");

        RuleFor(x => x.Password)
            .NotNull().WithMessage("Password is required.")
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.");
    }
    
    private async Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken)
    {
        return await _userRepository.IsEmailUniqueAsync(email, cancellationToken);
    }
}