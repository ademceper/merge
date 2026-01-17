using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetActivePreOrderCampaigns;

public class GetActivePreOrderCampaignsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetActivePreOrderCampaignsQuery>
{
    private readonly PaginationSettings config = paginationSettings.Value;

    public GetActivePreOrderCampaignsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxPageSize).WithMessage($"Sayfa boyutu en fazla {config.MaxPageSize} olabilir.");
    }
}

