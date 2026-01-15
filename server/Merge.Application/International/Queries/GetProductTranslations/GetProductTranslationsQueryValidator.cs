using FluentValidation;

namespace Merge.Application.International.Queries.GetProductTranslations;

public class GetProductTranslationsQueryValidator : AbstractValidator<GetProductTranslationsQuery>
{
    public GetProductTranslationsQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID'si zorunludur.");
    }
}

