using FluentValidation;

namespace Merge.Application.Catalog.Queries.GetInventoriesByProductId;

public class GetInventoriesByProductIdQueryValidator : AbstractValidator<GetInventoriesByProductIdQuery>
{
    public GetInventoriesByProductIdQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");
    }
}

