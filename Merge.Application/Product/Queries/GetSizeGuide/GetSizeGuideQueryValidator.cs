using FluentValidation;

namespace Merge.Application.Product.Queries.GetSizeGuide;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetSizeGuideQueryValidator : AbstractValidator<GetSizeGuideQuery>
{
    public GetSizeGuideQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Beden kılavuzu ID boş olamaz.");
    }
}
