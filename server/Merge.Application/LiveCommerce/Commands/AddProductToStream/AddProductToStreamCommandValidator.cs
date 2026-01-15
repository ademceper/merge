using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.AddProductToStream;

public class AddProductToStreamCommandValidator : AbstractValidator<AddProductToStreamCommand>
{
    public AddProductToStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Görüntüleme sırası 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.SpecialPrice)
            .GreaterThanOrEqualTo(0).When(x => x.SpecialPrice.HasValue)
            .WithMessage("Özel fiyat 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.ShowcaseNotes)
            .MaximumLength(1000).WithMessage("Vitrin notları en fazla 1000 karakter olabilir.");
    }
}
