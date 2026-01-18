using FluentValidation;

namespace Merge.Application.Seller.Commands.DeleteCommissionTier;

public class DeleteCommissionTierCommandValidator : AbstractValidator<DeleteCommissionTierCommand>
{
    public DeleteCommissionTierCommandValidator()
    {
        RuleFor(x => x.TierId)
            .NotEmpty().WithMessage("Tier ID is required.");
    }
}
