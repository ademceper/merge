using FluentValidation;

namespace Merge.Application.Catalog.Queries.GetAvailableStock;

public class GetAvailableStockQueryValidator : AbstractValidator<GetAvailableStockQuery>
{
    public GetAvailableStockQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");
    }
}

