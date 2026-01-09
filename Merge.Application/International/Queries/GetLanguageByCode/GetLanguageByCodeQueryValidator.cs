using FluentValidation;

namespace Merge.Application.International.Queries.GetLanguageByCode;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetLanguageByCodeQueryValidator : AbstractValidator<GetLanguageByCodeQuery>
{
    public GetLanguageByCodeQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(2, 10).WithMessage("Dil kodu en az 2, en fazla 10 karakter olmalıdır.");
    }
}

