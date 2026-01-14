using FluentValidation;

namespace Merge.Application.Catalog.Queries.GetAvailableStock;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetAvailableStockQueryValidator : AbstractValidator<GetAvailableStockQuery>
{
    public GetAvailableStockQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");
    }
}

