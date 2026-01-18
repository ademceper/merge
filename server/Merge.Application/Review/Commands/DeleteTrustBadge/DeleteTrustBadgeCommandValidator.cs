using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.DeleteTrustBadge;

public class DeleteTrustBadgeCommandValidator : AbstractValidator<DeleteTrustBadgeCommand>
{
    public DeleteTrustBadgeCommandValidator()
    {
        RuleFor(x => x.BadgeId)
            .NotEmpty()
            .WithMessage("Badge ID'si zorunludur.");
    }
}
