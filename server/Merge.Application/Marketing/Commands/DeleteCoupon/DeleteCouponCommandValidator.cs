using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteCoupon;

public class DeleteCouponCommandValidator : AbstractValidator<DeleteCouponCommand>
{
    public DeleteCouponCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kupon ID'si zorunludur.");
    }
}
