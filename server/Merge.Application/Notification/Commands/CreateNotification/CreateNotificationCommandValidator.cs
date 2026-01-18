using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.CreateNotification;


public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Bildirim başlığı zorunludur.")
            .MaximumLength(200)
            .WithMessage("Bildirim başlığı en fazla 200 karakter olabilir.")
            .MinimumLength(2)
            .WithMessage("Bildirim başlığı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Bildirim mesajı zorunludur.")
            .MaximumLength(2000)
            .WithMessage("Bildirim mesajı en fazla 2000 karakter olabilir.")
            .MinimumLength(1)
            .WithMessage("Bildirim mesajı en az 1 karakter olmalıdır.");

        RuleFor(x => x.Link)
            .MaximumLength(500)
            .WithMessage("Link en fazla 500 karakter olabilir.")
            .Must(link => string.IsNullOrEmpty(link) || Uri.IsWellFormedUriString(link, UriKind.Absolute))
            .WithMessage("Geçerli bir URL giriniz.")
            .When(x => !string.IsNullOrEmpty(x.Link));
    }
}
