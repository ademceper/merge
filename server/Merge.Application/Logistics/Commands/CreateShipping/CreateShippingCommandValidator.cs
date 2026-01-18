using FluentValidation;

namespace Merge.Application.Logistics.Commands.CreateShipping;

public class CreateShippingCommandValidator : AbstractValidator<CreateShippingCommand>
{
    public CreateShippingCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Sipariş ID'si zorunludur.");

        RuleFor(x => x.ShippingProvider)
            .NotEmpty().WithMessage("Kargo sağlayıcısı zorunludur.")
            .MaximumLength(100).WithMessage("Kargo sağlayıcısı en fazla 100 karakter olabilir.")
            .MinimumLength(2).WithMessage("Kargo sağlayıcısı en az 2 karakter olmalıdır.");

        RuleFor(x => x.ShippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("Kargo maliyeti 0 veya daha büyük olmalıdır.");
    }
}

