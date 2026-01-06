using FluentValidation;
using Merge.Application.Configuration;

namespace Merge.Application.Cart.Commands.CreatePreOrder;

public class CreatePreOrderCommandValidator : AbstractValidator<CreatePreOrderCommand>
{
    public CreatePreOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID zorunludur.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar en az 1 olmalıdır.");

        RuleFor(x => x.VariantOptions)
            .MaximumLength(500).WithMessage("Varyant seçenekleri en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.VariantOptions));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notlar en fazla 1000 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

