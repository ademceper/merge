using FluentValidation;

namespace Merge.Application.Seller.Commands.UpdateSellerCommissionSettings;

public class UpdateSellerCommissionSettingsCommandValidator : AbstractValidator<UpdateSellerCommissionSettingsCommand>
{
    public UpdateSellerCommissionSettingsCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required.");

        When(x => x.CustomCommissionRate.HasValue, () =>
        {
            RuleFor(x => x.CustomCommissionRate!.Value)
                .InclusiveBetween(0, 100).WithMessage("Custom commission rate must be between 0 and 100.");
        });

        When(x => x.MinimumPayoutAmount.HasValue, () =>
        {
            RuleFor(x => x.MinimumPayoutAmount!.Value)
                .GreaterThan(0).WithMessage("Minimum payout amount must be greater than 0.");
        });
    }
}
