using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Catalog.Queries.GetLowStockAlerts;

public class GetLowStockAlertsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetLowStockAlertsQuery>
{
    private readonly PaginationSettings config = paginationSettings.Value;

    public GetLowStockAlertsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
// Note: PerformedBy is optional - Admin can see all alerts, Seller can only see their own
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

