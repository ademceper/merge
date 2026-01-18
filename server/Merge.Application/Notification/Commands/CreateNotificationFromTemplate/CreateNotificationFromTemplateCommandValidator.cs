using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.CreateNotificationFromTemplate;


public class CreateNotificationFromTemplateCommandValidator : AbstractValidator<CreateNotificationFromTemplateCommand>
{
    public CreateNotificationFromTemplateCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.TemplateType)
            .IsInEnum()
            .WithMessage("Geçerli bir şablon tipi seçiniz.");
    }
}
