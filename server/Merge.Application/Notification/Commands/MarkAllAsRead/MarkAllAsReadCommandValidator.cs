using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.MarkAllAsRead;


public class MarkAllAsReadCommandValidator : AbstractValidator<MarkAllAsReadCommand>
{
    public MarkAllAsReadCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
