using FluentValidation;

namespace Merge.Application.International.Queries.GetCurrencyById;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetCurrencyByIdQueryValidator : AbstractValidator<GetCurrencyByIdQuery>
{
    public GetCurrencyByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Para birimi ID'si zorunludur.");
    }
}

