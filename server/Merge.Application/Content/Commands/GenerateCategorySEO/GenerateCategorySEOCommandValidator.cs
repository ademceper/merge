using FluentValidation;

namespace Merge.Application.Content.Commands.GenerateCategorySEO;

public class GenerateCategorySEOCommandValidator : AbstractValidator<GenerateCategorySEOCommand>
{
    public GenerateCategorySEOCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Kategori ID'si zorunludur.");
    }
}

