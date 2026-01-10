using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteCoupon;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class DeleteCouponCommandValidator : AbstractValidator<DeleteCouponCommand>
{
    public DeleteCouponCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kupon ID'si zorunludur.");
    }
}
