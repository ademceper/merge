using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.ExportProductsToCsv;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class ExportProductsToCsvQueryValidator : AbstractValidator<ExportProductsToCsvQuery>
{
    public ExportProductsToCsvQueryValidator()
    {
        RuleFor(x => x.ExportDto)
            .NotNull().WithMessage("Export DTO is required");
    }
}
