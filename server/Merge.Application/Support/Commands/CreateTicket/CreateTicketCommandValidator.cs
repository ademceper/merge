using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Support.Commands.CreateTicket;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class CreateTicketCommandValidator(IOptions<SupportSettings> settings) : AbstractValidator<CreateTicketCommand>
{
    private readonly SupportSettings config = settings.Value;

    public CreateTicketCommandValidator() : this(Options.Create(new SupportSettings()))
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Kategori boş olamaz")
            .MaximumLength(config.MaxCategoryEnumLength).WithMessage($"Kategori en fazla {config.MaxCategoryEnumLength} karakter olmalıdır");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Öncelik boş olamaz")
            .MaximumLength(config.MaxPriorityEnumLength).WithMessage($"Öncelik en fazla {config.MaxPriorityEnumLength} karakter olmalıdır");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Konu boş olamaz")
            .MinimumLength(config.MinTicketSubjectLength).WithMessage($"Konu en az {config.MinTicketSubjectLength} karakter olmalıdır")
            .MaximumLength(config.MaxTicketSubjectLength)
            .WithMessage($"Konu en fazla {config.MaxTicketSubjectLength} karakter olmalıdır");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Açıklama boş olamaz")
            .MinimumLength(config.MinTicketDescriptionLength).WithMessage($"Açıklama en az {config.MinTicketDescriptionLength} karakter olmalıdır")
            .MaximumLength(config.MaxTicketDescriptionLength)
            .WithMessage($"Açıklama en fazla {config.MaxTicketDescriptionLength} karakter olmalıdır");
    }
}
