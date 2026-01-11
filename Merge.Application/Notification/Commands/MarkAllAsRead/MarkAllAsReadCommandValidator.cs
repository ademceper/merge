using FluentValidation;

namespace Merge.Application.Notification.Commands.MarkAllAsRead;

/// <summary>
/// Mark All As Read Command Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class MarkAllAsReadCommandValidator : AbstractValidator<MarkAllAsReadCommand>
{
    public MarkAllAsReadCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
