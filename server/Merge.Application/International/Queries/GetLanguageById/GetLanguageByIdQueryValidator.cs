using FluentValidation;

namespace Merge.Application.International.Queries.GetLanguageById;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetLanguageByIdQueryValidator : AbstractValidator<GetLanguageByIdQuery>
{
    public GetLanguageByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Dil ID'si zorunludur.");
    }
}

