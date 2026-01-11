using FluentValidation;

namespace Merge.Application.Seller.Commands.CreateCommissionTier;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class CreateCommissionTierCommandValidator : AbstractValidator<CreateCommissionTierCommand>
{
    public CreateCommissionTierCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tier name is required.")
            .MaximumLength(100).WithMessage("Tier name must not exceed 100 characters.");

        RuleFor(x => x.MinSales)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum sales must be greater than or equal to 0.");

        RuleFor(x => x.MaxSales)
            .GreaterThan(x => x.MinSales).WithMessage("Maximum sales must be greater than minimum sales.");

        RuleFor(x => x.CommissionRate)
            .InclusiveBetween(0, 100).WithMessage("Commission rate must be between 0 and 100.");

        RuleFor(x => x.PlatformFeeRate)
            .InclusiveBetween(0, 100).WithMessage("Platform fee rate must be between 0 and 100.");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be greater than or equal to 0.");
    }
}
