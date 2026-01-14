using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Catalog.Queries.GetLowStockAlerts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetLowStockAlertsQueryValidator : AbstractValidator<GetLowStockAlertsQuery>
{
    public GetLowStockAlertsQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var settings = paginationSettings.Value;

        // Note: PerformedBy is optional - Admin can see all alerts, Seller can only see their own
        // Validation is done at handler level for IDOR protection

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Sayfa numarası 1'den büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Sayfa boyutu 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir.");
    }
}

