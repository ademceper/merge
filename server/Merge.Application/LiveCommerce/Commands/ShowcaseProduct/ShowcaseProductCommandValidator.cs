using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.ShowcaseProduct;

public class ShowcaseProductCommandValidator : AbstractValidator<ShowcaseProductCommand>
{
    public ShowcaseProductCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID'si zorunludur.");
    }
}
