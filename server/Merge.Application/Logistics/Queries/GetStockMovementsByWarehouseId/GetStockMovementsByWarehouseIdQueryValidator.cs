using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetStockMovementsByWarehouseId;

public class GetStockMovementsByWarehouseIdQueryValidator : AbstractValidator<GetStockMovementsByWarehouseIdQuery>
{
    public GetStockMovementsByWarehouseIdQueryValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Depo ID'si zorunludur.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir.");
    }
}

