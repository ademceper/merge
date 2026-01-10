using FluentValidation;

namespace Merge.Application.ML.Queries.GetPriceRecommendation;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetPriceRecommendationQueryValidator : AbstractValidator<GetPriceRecommendationQuery>
{
    public GetPriceRecommendationQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");
    }
}
