using FluentValidation;

namespace Merge.Application.Product.Queries.ExportProductsToExcel;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class ExportProductsToExcelQueryValidator : AbstractValidator<ExportProductsToExcelQuery>
{
    public ExportProductsToExcelQueryValidator()
    {
        RuleFor(x => x.ExportDto)
            .NotNull().WithMessage("Export DTO is required");
    }
}
