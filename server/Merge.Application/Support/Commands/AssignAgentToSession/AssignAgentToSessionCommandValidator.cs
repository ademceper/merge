using FluentValidation;

namespace Merge.Application.Support.Commands.AssignAgentToSession;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class AssignAgentToSessionCommandValidator : AbstractValidator<AssignAgentToSessionCommand>
{
    public AssignAgentToSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Oturum ID boş olamaz");

        RuleFor(x => x.AgentId)
            .NotEmpty().WithMessage("Agent ID boş olamaz");
    }
}
