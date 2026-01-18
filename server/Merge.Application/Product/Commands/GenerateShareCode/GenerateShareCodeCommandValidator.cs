using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.GenerateShareCode;

public class GenerateShareCodeCommandValidator : AbstractValidator<GenerateShareCodeCommand>
{
    public GenerateShareCodeCommandValidator()
    {
        RuleFor(x => x.ComparisonId)
            .NotEmpty().WithMessage("Comparison ID is required");
    }
}
