using FluentValidation;

namespace Merge.Application.Seller.Commands.CancelCommission;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class CancelCommissionCommandValidator : AbstractValidator<CancelCommissionCommand>
{
    public CancelCommissionCommandValidator()
    {
        RuleFor(x => x.CommissionId)
            .NotEmpty().WithMessage("Commission ID is required.");
    }
}
