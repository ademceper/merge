using FluentValidation;

namespace Merge.Application.Content.Commands.UpdateSitemapEntry;

public class UpdateSitemapEntryCommandValidator : AbstractValidator<UpdateSitemapEntryCommand>
{
    public UpdateSitemapEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Sitemap entry ID'si zorunludur.");

        RuleFor(x => x.Url)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Url))
            .WithMessage("URL en fazla 500 karakter olabilir.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 1)
            .When(x => x.Priority.HasValue)
            .WithMessage("Priority 0.0 ile 1.0 arasında olmalıdır.");

        RuleFor(x => x.ChangeFrequency)
            .Must(freq => string.IsNullOrEmpty(freq) || 
                new[] { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" }
                    .Contains(freq.ToLowerInvariant()))
            .When(x => !string.IsNullOrEmpty(x.ChangeFrequency))
            .WithMessage("Geçersiz change frequency. Geçerli değerler: always, hourly, daily, weekly, monthly, yearly, never");
    }
}

