using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Support.Commands.CreateTicket;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator(IOptions<SupportSettings> settings)
    {
        var supportSettings = settings.Value;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Kategori boş olamaz")
            .MaximumLength(supportSettings.MaxCategoryEnumLength).WithMessage($"Kategori en fazla {supportSettings.MaxCategoryEnumLength} karakter olmalıdır");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Öncelik boş olamaz")
            .MaximumLength(supportSettings.MaxPriorityEnumLength).WithMessage($"Öncelik en fazla {supportSettings.MaxPriorityEnumLength} karakter olmalıdır");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Konu boş olamaz")
            .MinimumLength(supportSettings.MinTicketSubjectLength).WithMessage($"Konu en az {supportSettings.MinTicketSubjectLength} karakter olmalıdır")
            .MaximumLength(supportSettings.MaxTicketSubjectLength)
            .WithMessage($"Konu en fazla {supportSettings.MaxTicketSubjectLength} karakter olmalıdır");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Açıklama boş olamaz")
            .MinimumLength(supportSettings.MinTicketDescriptionLength).WithMessage($"Açıklama en az {supportSettings.MinTicketDescriptionLength} karakter olmalıdır")
            .MaximumLength(supportSettings.MaxTicketDescriptionLength)
            .WithMessage($"Açıklama en fazla {supportSettings.MaxTicketDescriptionLength} karakter olmalıdır");
    }
}
