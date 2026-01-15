using FluentValidation;

namespace Merge.Application.International.Queries.GetCurrencyById;

public class GetCurrencyByIdQueryValidator : AbstractValidator<GetCurrencyByIdQuery>
{
    public GetCurrencyByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Para birimi ID'si zorunludur.");
    }
}

