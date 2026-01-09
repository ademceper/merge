using FluentValidation;

namespace Merge.Application.International.Queries.GetAllLanguages;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetAllLanguagesQueryValidator : AbstractValidator<GetAllLanguagesQuery>
{
    public GetAllLanguagesQueryValidator()
    {
        // GetAllLanguagesQuery parametre almadığı için validator boş
        // Ancak FluentValidation pipeline'ı için gerekli
    }
}

