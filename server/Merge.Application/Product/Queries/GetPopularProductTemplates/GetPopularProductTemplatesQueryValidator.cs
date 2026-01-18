using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetPopularProductTemplates;

public class GetPopularProductTemplatesQueryValidator : AbstractValidator<GetPopularProductTemplatesQuery>
{
    public GetPopularProductTemplatesQueryValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Limit en fazla 100 olabilir.");
    }
}
