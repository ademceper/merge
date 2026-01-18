using FluentValidation;

namespace Merge.Application.Seller.Commands.CancelCommission;

public class CancelCommissionCommandValidator : AbstractValidator<CancelCommissionCommand>
{
    public CancelCommissionCommandValidator()
    {
        RuleFor(x => x.CommissionId)
            .NotEmpty().WithMessage("Commission ID is required.");
    }
}
