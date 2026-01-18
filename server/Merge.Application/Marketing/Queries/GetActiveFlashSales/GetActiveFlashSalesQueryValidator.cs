using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetActiveFlashSales;

public class GetActiveFlashSalesQueryValidator : AbstractValidator<GetActiveFlashSalesQuery>
{
    public GetActiveFlashSalesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir.");
    }
}
