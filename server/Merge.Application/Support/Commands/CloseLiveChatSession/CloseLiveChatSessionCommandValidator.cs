using FluentValidation;

namespace Merge.Application.Support.Commands.CloseLiveChatSession;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class CloseLiveChatSessionCommandValidator : AbstractValidator<CloseLiveChatSessionCommand>
{
    public CloseLiveChatSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Oturum ID boş olamaz");
    }
}
