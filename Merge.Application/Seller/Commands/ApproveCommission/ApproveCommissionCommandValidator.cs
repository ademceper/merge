using FluentValidation;

namespace Merge.Application.Seller.Commands.ApproveCommission;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ApproveCommissionCommandValidator : AbstractValidator<ApproveCommissionCommand>
{
    public ApproveCommissionCommandValidator()
    {
        RuleFor(x => x.CommissionId)
            .NotEmpty().WithMessage("Commission ID is required.");
    }
}
