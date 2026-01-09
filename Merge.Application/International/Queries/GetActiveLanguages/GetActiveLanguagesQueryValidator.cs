using FluentValidation;

namespace Merge.Application.International.Queries.GetActiveLanguages;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetActiveLanguagesQueryValidator : AbstractValidator<GetActiveLanguagesQuery>
{
    public GetActiveLanguagesQueryValidator()
    {
        // GetActiveLanguagesQuery parametre almadığı için validator boş
        // Ancak FluentValidation pipeline'ı için gerekli
    }
}

