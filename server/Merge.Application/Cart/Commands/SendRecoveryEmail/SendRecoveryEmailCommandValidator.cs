using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.SendRecoveryEmail;

public class SendRecoveryEmailCommandValidator : AbstractValidator<SendRecoveryEmailCommand>
{
    public SendRecoveryEmailCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Sepet ID zorunludur");

        RuleFor(x => x.CouponDiscountPercentage)
            .InclusiveBetween(0, 100)
            .When(x => x.CouponDiscountPercentage.HasValue)
            .WithMessage("Kupon indirim yüzdesi 0 ile 100 arasında olmalıdır");
    }
}

