using FluentValidation;

namespace Merge.Application.International.Queries.GetAllLanguages;

public class GetAllLanguagesQueryValidator : AbstractValidator<GetAllLanguagesQuery>
{
    public GetAllLanguagesQueryValidator()
    {
        // GetAllLanguagesQuery parametre almadığı için validator boş
        // Ancak FluentValidation pipeline'ı için gerekli
    }
}

