using FluentValidation;

namespace Item.Application.Features.CopyFile;

public class CopyFileValidator : AbstractValidator<CopyFileRequestDto>
{
    public CopyFileValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("Name is required.")
            .NotEmpty().WithMessage("Name is required.");
    }
}