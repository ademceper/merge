using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.DeleteTemplate;

/// <summary>
/// Delete Template Command Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class DeleteTemplateCommandValidator : AbstractValidator<DeleteTemplateCommand>
{
    public DeleteTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Şablon ID'si zorunludur.");
    }
}
