using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.B2B.Queries.GetOrganizationPurchaseOrders;

public class GetOrganizationPurchaseOrdersQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetOrganizationPurchaseOrdersQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetOrganizationPurchaseOrdersQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organizasyon ID boş olamaz");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(settings.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir");
    }
}

