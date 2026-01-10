using FluentValidation;

namespace Merge.Application.Marketing.Queries.CalculatePointsFromPurchase;

public class CalculatePointsFromPurchaseQueryValidator : AbstractValidator<CalculatePointsFromPurchaseQuery>
{
    public CalculatePointsFromPurchaseQueryValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Sipariş tutarı 0'dan büyük olmalıdır.");
    }
}
