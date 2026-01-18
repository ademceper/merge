using FluentValidation;

namespace Merge.Application.Support.Commands.MarkMessagesAsRead;

public class MarkMessagesAsReadCommandValidator : AbstractValidator<MarkMessagesAsReadCommand>
{
    public MarkMessagesAsReadCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Oturum ID boş olamaz");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz");
    }
}
