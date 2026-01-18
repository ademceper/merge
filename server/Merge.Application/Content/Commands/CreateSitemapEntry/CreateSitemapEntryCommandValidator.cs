using FluentValidation;

namespace Merge.Application.Content.Commands.CreateSitemapEntry;

public class CreateSitemapEntryCommandValidator : AbstractValidator<CreateSitemapEntryCommand>
{
    public CreateSitemapEntryCommandValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage("URL zorunludur.")
            .MaximumLength(500)
            .WithMessage("URL en fazla 500 karakter olabilir.");

        RuleFor(x => x.PageType)
            .NotEmpty()
            .WithMessage("Sayfa tipi zorunludur.")
            .MaximumLength(50)
            .WithMessage("Sayfa tipi en fazla 50 karakter olabilir.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 1)
            .WithMessage("Priority 0.0 ile 1.0 arasında olmalıdır.");

        RuleFor(x => x.ChangeFrequency)
            .Must(freq => new[] { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" }
                .Contains(freq.ToLowerInvariant()))
            .WithMessage("Geçersiz change frequency. Geçerli değerler: always, hourly, daily, weekly, monthly, yearly, never");
    }
}

