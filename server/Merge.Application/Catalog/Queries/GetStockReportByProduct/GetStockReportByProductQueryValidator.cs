using FluentValidation;

namespace Merge.Application.Catalog.Queries.GetStockReportByProduct;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetStockReportByProductQueryValidator : AbstractValidator<GetStockReportByProductQuery>
{
    public GetStockReportByProductQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");
    }
}

