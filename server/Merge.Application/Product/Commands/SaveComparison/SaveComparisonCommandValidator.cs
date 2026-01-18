using FluentValidation;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Product.Commands.SaveComparison;

public class SaveComparisonCommandValidator : AbstractValidator<SaveComparisonCommand>
{
    public SaveComparisonCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));
    }
}
