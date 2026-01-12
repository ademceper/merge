using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetActivePreOrderCampaigns;

public class GetActivePreOrderCampaignsQueryValidator : AbstractValidator<GetActivePreOrderCampaignsQuery>
{
    public GetActivePreOrderCampaignsQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var settings = paginationSettings.Value;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxPageSize).WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir.");
    }
}

