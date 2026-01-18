using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetSizeGuidesByCategory;

public class GetSizeGuidesByCategoryQueryValidator : AbstractValidator<GetSizeGuidesByCategoryQuery>
{
    public GetSizeGuidesByCategoryQueryValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori ID boş olamaz.");
    }
}
