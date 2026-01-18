using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.CreateTemplate;


public class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage("Şablon bilgileri zorunludur.");

        When(x => x.Dto is not null, () =>
        {
            RuleFor(x => x.Dto.Name)
                .NotEmpty()
                .WithMessage("Şablon adı zorunludur.")
                .MaximumLength(100)
                .WithMessage("Şablon adı en fazla 100 karakter olabilir.")
                .MinimumLength(2)
                .WithMessage("Şablon adı en az 2 karakter olmalıdır.");

            RuleFor(x => x.Dto.Type)
                .IsInEnum()
                .WithMessage("Geçerli bir bildirim tipi seçiniz.");

            RuleFor(x => x.Dto.TitleTemplate)
                .NotEmpty()
                .WithMessage("Başlık şablonu zorunludur.")
                .MaximumLength(500)
                .WithMessage("Başlık şablonu en fazla 500 karakter olabilir.");

            RuleFor(x => x.Dto.MessageTemplate)
                .NotEmpty()
                .WithMessage("Mesaj şablonu zorunludur.")
                .MaximumLength(4000)
                .WithMessage("Mesaj şablonu en fazla 4000 karakter olabilir.");

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
