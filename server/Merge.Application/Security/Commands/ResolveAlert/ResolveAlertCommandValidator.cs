using FluentValidation;

namespace Merge.Application.Security.Commands.ResolveAlert;

public class ResolveAlertCommandValidator : AbstractValidator<ResolveAlertCommand>
{
    public ResolveAlertCommandValidator()
    {
        RuleFor(x => x.AlertId)
            .NotEmpty().WithMessage("AlertId is required");

        RuleFor(x => x.ResolvedByUserId)
            .NotEmpty().WithMessage("ResolvedByUserId is required");

        RuleFor(x => x.ResolutionNotes)
            .MaximumLength(2000).WithMessage("ResolutionNotes cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.ResolutionNotes));
    }
}
