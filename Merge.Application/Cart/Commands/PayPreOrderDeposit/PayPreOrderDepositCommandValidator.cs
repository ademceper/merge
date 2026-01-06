using FluentValidation;

namespace Merge.Application.Cart.Commands.PayPreOrderDeposit;

public class PayPreOrderDepositCommandValidator : AbstractValidator<PayPreOrderDepositCommand>
{
    public PayPreOrderDepositCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");

        RuleFor(x => x.PreOrderId)
            .NotEmpty().WithMessage("Ön sipariş ID zorunludur.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Tutar 0'dan büyük olmalıdır.");
    }
}

