using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Queries.GetFilteredStockMovements;

public class GetFilteredStockMovementsQueryValidator : AbstractValidator<GetFilteredStockMovementsQuery>
{
    public GetFilteredStockMovementsQueryValidator()
    {
        RuleFor(x => x.MovementType)
            .IsInEnum()
            .When(x => x.MovementType.HasValue)
            .WithMessage("Geçerli bir hareket tipi seçiniz.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Bitiş tarihi başlangıç tarihinden önce olamaz.");
    }
}

