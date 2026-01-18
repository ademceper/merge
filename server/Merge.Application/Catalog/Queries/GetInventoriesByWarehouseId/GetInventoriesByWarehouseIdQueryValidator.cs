using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Catalog.Queries.GetInventoriesByWarehouseId;

public class GetInventoriesByWarehouseIdQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetInventoriesByWarehouseIdQuery>
{
    private readonly PaginationSettings config = paginationSettings.Value;

    public GetInventoriesByWarehouseIdQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .WithMessage("Depo ID'si zorunludur.");

        // Note: PerformedBy is optional - Admin can see all inventories, Seller can only see their own
        // Validation is done at handler level for IDOR protection

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

