using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetUserPreOrders;

public class GetUserPreOrdersQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetUserPreOrdersQuery>
{
    private readonly PaginationSettings config = paginationSettings.Value;

    public GetUserPreOrdersQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxPageSize).WithMessage($"Sayfa boyutu en fazla {config.MaxPageSize} olabilir.");
    }
}

