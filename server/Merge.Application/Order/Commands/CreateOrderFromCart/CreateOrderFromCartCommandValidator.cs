using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CreateOrderFromCart;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateOrderFromCartCommandValidator : AbstractValidator<CreateOrderFromCartCommand>
{
    public CreateOrderFromCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.AddressId)
            .NotEmpty()
            .WithMessage("Adres ID'si zorunludur.");

        RuleFor(x => x.CouponCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.CouponCode))
            .WithMessage("Kupon kodu en fazla 50 karakter olabilir.");
    }
}
