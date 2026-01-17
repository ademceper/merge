using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetAllProducts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetAllProductsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetAllProductsQuery>
{
    private readonly PaginationSettings config = paginationSettings.Value;

    public GetAllProductsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Sayfa numarası 1'den büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Sayfa boyutu 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {config.MaxPageSize} olabilir.");
    }
}
