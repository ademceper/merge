using FluentValidation;

namespace Merge.Application.International.Queries.GetActiveCurrencies;

public class GetActiveCurrenciesQueryValidator : AbstractValidator<GetActiveCurrenciesQuery>
{
    public GetActiveCurrenciesQueryValidator()
    {
        // GetActiveCurrenciesQuery parametre almadığı için validator boş
        // Ancak FluentValidation pipeline'ı için gerekli
    }
}

