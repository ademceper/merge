using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetUserPreOrders;

public class GetUserPreOrdersQueryValidator : AbstractValidator<GetUserPreOrdersQuery>
{
    public GetUserPreOrdersQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var settings = paginationSettings.Value;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxPageSize).WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir.");
    }
}

