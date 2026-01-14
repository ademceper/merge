using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Commands.DeleteOldActivities;

public class DeleteOldActivitiesCommandValidator : AbstractValidator<DeleteOldActivitiesCommand>
{
    public DeleteOldActivitiesCommandValidator()
    {
        RuleFor(x => x.DaysToKeep)
            .GreaterThan(0)
            .WithMessage("Tutulacak gün sayısı 0'dan büyük olmalıdır.");
    }
}
