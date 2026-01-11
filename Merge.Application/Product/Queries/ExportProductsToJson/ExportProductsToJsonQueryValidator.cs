using FluentValidation;

namespace Merge.Application.Product.Queries.ExportProductsToJson;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class ExportProductsToJsonQueryValidator : AbstractValidator<ExportProductsToJsonQuery>
{
    public ExportProductsToJsonQueryValidator()
    {
        RuleFor(x => x.ExportDto)
            .NotNull().WithMessage("Export DTO is required");
    }
}
