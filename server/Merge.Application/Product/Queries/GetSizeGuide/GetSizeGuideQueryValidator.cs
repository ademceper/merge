using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetSizeGuide;

public class GetSizeGuideQueryValidator : AbstractValidator<GetSizeGuideQuery>
{
    public GetSizeGuideQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Beden kılavuzu ID boş olamaz.");
    }
}
