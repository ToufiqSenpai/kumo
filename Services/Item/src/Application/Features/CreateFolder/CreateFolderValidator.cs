using FluentValidation;
using Item.Application.Repositories;

namespace Item.Application.Features.CreateFolder;

public sealed class CreateFolderValidator : AbstractValidator<CreateFolderRequestDto>
{
    public CreateFolderValidator(IItemRepository repository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        
        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters.");
    }
}