using FluentValidation;

namespace Merge.Application.Seller.Commands.RequestPayout;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class RequestPayoutCommandValidator : AbstractValidator<RequestPayoutCommand>
{
    public RequestPayoutCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required.");

        RuleFor(x => x.CommissionIds)
            .NotEmpty().WithMessage("At least one commission ID is required.")
            .Must(ids => ids.Count > 0).WithMessage("At least one commission ID is required.");

        RuleForEach(x => x.CommissionIds)
            .NotEmpty().WithMessage("Commission ID cannot be empty.");
    }
}
