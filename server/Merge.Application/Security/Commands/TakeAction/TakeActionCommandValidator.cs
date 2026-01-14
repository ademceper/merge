using FluentValidation;

namespace Merge.Application.Security.Commands.TakeAction;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class TakeActionCommandValidator : AbstractValidator<TakeActionCommand>
{
    public TakeActionCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("EventId is required");

        RuleFor(x => x.ActionTakenByUserId)
            .NotEmpty().WithMessage("ActionTakenByUserId is required");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required")
            .MaximumLength(200).WithMessage("Action cannot exceed 200 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
