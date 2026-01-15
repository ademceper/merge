using FluentValidation;

namespace Merge.Application.International.Queries.GetActiveLanguages;

public class GetActiveLanguagesQueryValidator() : AbstractValidator<GetActiveLanguagesQuery>
{
    public GetActiveLanguagesQueryValidator()
    {
        // GetActiveLanguagesQuery parametre almadığı için validator boş
        // Ancak FluentValidation pipeline'ı için gerekli
    }
}

