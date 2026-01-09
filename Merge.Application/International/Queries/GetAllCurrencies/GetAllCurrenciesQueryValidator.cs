using FluentValidation;

namespace Merge.Application.International.Queries.GetAllCurrencies;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetAllCurrenciesQueryValidator : AbstractValidator<GetAllCurrenciesQuery>
{
    public GetAllCurrenciesQueryValidator()
    {
        // GetAllCurrenciesQuery parametre almadığı için validator boş
        // Ancak FluentValidation pipeline'ı için gerekli
    }
}

