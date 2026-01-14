using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.CreateNotificationFromTemplate;

/// <summary>
/// Create Notification From Template Command Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
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
