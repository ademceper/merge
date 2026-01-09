using FluentValidation;

namespace Merge.Application.Content.Queries.GetSEOSettings;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetSEOSettingsQueryValidator : AbstractValidator<GetSEOSettingsQuery>
{
    public GetSEOSettingsQueryValidator()
    {
        RuleFor(x => x.PageType)
            .NotEmpty()
            .WithMessage("Sayfa tipi zorunludur.")
            .MaximumLength(50)
            .WithMessage("Sayfa tipi en fazla 50 karakter olabilir.");
    }
}

