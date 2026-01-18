using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.ExportProductsToJson;

public class ExportProductsToJsonQueryValidator : AbstractValidator<ExportProductsToJsonQuery>
{
    public ExportProductsToJsonQueryValidator()
    {
        RuleFor(x => x.ExportDto)
            .NotNull().WithMessage("Export DTO is required");
    }
}
