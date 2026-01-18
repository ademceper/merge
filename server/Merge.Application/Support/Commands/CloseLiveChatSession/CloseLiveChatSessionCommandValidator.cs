using FluentValidation;

namespace Merge.Application.Support.Commands.CloseLiveChatSession;

public class CloseLiveChatSessionCommandValidator : AbstractValidator<CloseLiveChatSessionCommand>
{
    public CloseLiveChatSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Oturum ID boş olamaz");
    }
}
