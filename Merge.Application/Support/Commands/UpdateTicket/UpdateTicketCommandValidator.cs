using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.UpdateTicket;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class UpdateTicketCommandValidator : AbstractValidator<UpdateTicketCommand>
{
    public UpdateTicketCommandValidator(IOptions<SupportSettings> settings)
    {
        var supportSettings = settings.Value;

        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID boş olamaz");

        When(x => !string.IsNullOrEmpty(x.Subject), () =>
        {
            RuleFor(x => x.Subject)
                .MinimumLength(supportSettings.MinTicketSubjectLength).WithMessage($"Konu en az {supportSettings.MinTicketSubjectLength} karakter olmalıdır")
                .MaximumLength(supportSettings.MaxTicketSubjectLength)
                .WithMessage($"Konu en fazla {supportSettings.MaxTicketSubjectLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MinimumLength(supportSettings.MinTicketDescriptionLength).WithMessage($"Açıklama en az {supportSettings.MinTicketDescriptionLength} karakter olmalıdır")
                .MaximumLength(supportSettings.MaxTicketDescriptionLength)
                .WithMessage($"Açıklama en fazla {supportSettings.MaxTicketDescriptionLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.Category), () =>
        {
            RuleFor(x => x.Category)
                .MaximumLength(supportSettings.MaxCategoryEnumLength).WithMessage($"Kategori en fazla {supportSettings.MaxCategoryEnumLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.Priority), () =>
        {
            RuleFor(x => x.Priority)
                .MaximumLength(supportSettings.MaxPriorityEnumLength).WithMessage($"Öncelik en fazla {supportSettings.MaxPriorityEnumLength} karakter olmalıdır");
        });
    }
}
