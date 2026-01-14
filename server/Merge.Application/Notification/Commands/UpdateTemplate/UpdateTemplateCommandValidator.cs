using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.UpdateTemplate;

/// <summary>
/// Update Template Command Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Şablon ID'si zorunludur.");

        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage("Güncelleme bilgileri zorunludur.");

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto.Name)
                .MaximumLength(100)
                .WithMessage("Şablon adı en fazla 100 karakter olabilir.")
                .MinimumLength(2)
                .WithMessage("Şablon adı en az 2 karakter olmalıdır.")
                .When(x => !string.IsNullOrEmpty(x.Dto.Name));

            RuleFor(x => x.Dto.Type)
                .IsInEnum()
                .WithMessage("Geçerli bir bildirim tipi seçiniz.")
                .When(x => x.Dto.Type.HasValue);

            RuleFor(x => x.Dto.TitleTemplate)
                .MaximumLength(500)
                .WithMessage("Başlık şablonu en fazla 500 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.Dto.TitleTemplate));

            RuleFor(x => x.Dto.MessageTemplate)
                .MaximumLength(4000)
                .WithMessage("Mesaj şablonu en fazla 4000 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.Dto.MessageTemplate));

            RuleFor(x => x.Dto.LinkTemplate)
                .MaximumLength(500)
                .WithMessage("Link şablonu en fazla 500 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.Dto.LinkTemplate));

            RuleFor(x => x.Dto.Description)
                .MaximumLength(1000)
                .WithMessage("Açıklama en fazla 1000 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.Dto.Description));
        });
    }
}
