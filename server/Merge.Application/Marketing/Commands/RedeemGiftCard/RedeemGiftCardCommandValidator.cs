using FluentValidation;

namespace Merge.Application.Marketing.Commands.RedeemGiftCard;

public class RedeemGiftCardCommandValidator : AbstractValidator<RedeemGiftCardCommand>
{
    public RedeemGiftCardCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Hediye kartı kodu zorunludur.")
            .MaximumLength(50).WithMessage("Hediye kartı kodu en fazla 50 karakter olabilir.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
