using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.B2B.Queries.GetB2BUserPurchaseOrders;

public class GetB2BUserPurchaseOrdersQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetB2BUserPurchaseOrdersQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetB2BUserPurchaseOrdersQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.B2BUserId)
            .NotEmpty().WithMessage("B2B kullanıcı ID boş olamaz");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(settings.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir");
    }
}

