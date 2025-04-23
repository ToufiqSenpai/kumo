using System.Globalization;
using FluentValidation;
using Item.Application.Repositories;
using Item.Domain.Models;
using MassTransit.Serialization;

namespace Item.Application.Features.CreateFile;

public class CreateFileValidator : AbstractValidator<CreateFileRequestDto>
{
    public CreateFileValidator(IItemRepository itemRepository, IHttpContextAccessor httpContextAccessor)
    {
        // httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue()
        RuleFor(x => x.Name)
            .NotNull().WithMessage("Name is required.")
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(1).WithMessage("Name must be at least 1 characters.")
            .MaximumLength(255).WithMessage("Name must be between 1 and 255 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must be between 1 and 5000 characters.");
    }
}