using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Queries.GetAllPickPacks;

public class GetAllPickPacksQueryValidator : AbstractValidator<GetAllPickPacksQuery>
{
    public GetAllPickPacksQueryValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Geçerli bir pick-pack durumu seçiniz.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir.");
    }
}

