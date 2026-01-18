using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.ML.Queries.GetPriceRecommendation;

public class GetPriceRecommendationQueryValidator : AbstractValidator<GetPriceRecommendationQuery>
{
    public GetPriceRecommendationQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");
    }
}
