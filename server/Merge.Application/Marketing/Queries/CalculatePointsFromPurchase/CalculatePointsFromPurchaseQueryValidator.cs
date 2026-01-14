using FluentValidation;

namespace Merge.Application.Marketing.Queries.CalculatePointsFromPurchase;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class CalculatePointsFromPurchaseQueryValidator : AbstractValidator<CalculatePointsFromPurchaseQuery>
{
    public CalculatePointsFromPurchaseQueryValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Sipariş tutarı 0'dan büyük olmalıdır.");
    }
}
