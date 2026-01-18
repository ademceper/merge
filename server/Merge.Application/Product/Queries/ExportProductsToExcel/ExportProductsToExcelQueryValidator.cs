using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.ExportProductsToExcel;

public class ExportProductsToExcelQueryValidator : AbstractValidator<ExportProductsToExcelQuery>
{
    public ExportProductsToExcelQueryValidator()
    {
        RuleFor(x => x.ExportDto)
            .NotNull().WithMessage("Export DTO is required");
    }
}
