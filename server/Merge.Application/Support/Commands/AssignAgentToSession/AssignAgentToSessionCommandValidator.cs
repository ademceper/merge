using FluentValidation;

namespace Merge.Application.Support.Commands.AssignAgentToSession;

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
