using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.B2B.Queries.GetB2BUserPurchaseOrders;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetB2BUserPurchaseOrdersQueryValidator : AbstractValidator<GetB2BUserPurchaseOrdersQuery>
{
    public GetB2BUserPurchaseOrdersQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        RuleFor(x => x.B2BUserId)
            .NotEmpty().WithMessage("B2B kullanıcı ID boş olamaz");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(paginationSettings.Value.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {paginationSettings.Value.MaxPageSize} olabilir");
    }
}

