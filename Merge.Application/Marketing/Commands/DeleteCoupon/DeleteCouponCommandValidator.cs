using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteCoupon;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class DeleteCouponCommandValidator() : AbstractValidator<DeleteCouponCommand>
{
    public DeleteCouponCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kupon ID'si zorunludur.");
    }
}
