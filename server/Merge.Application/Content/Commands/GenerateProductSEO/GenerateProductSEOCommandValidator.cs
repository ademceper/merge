using FluentValidation;

namespace Merge.Application.Content.Commands.GenerateProductSEO;

public class GenerateProductSEOCommandValidator : AbstractValidator<GenerateProductSEOCommand>
{
    public GenerateProductSEOCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");
    }
}

