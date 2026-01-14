using FluentValidation;

namespace Merge.Application.Marketing.Queries.CalculateDiscountFromPoints;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class CalculateDiscountFromPointsQueryValidator : AbstractValidator<CalculateDiscountFromPointsQuery>
{
    public CalculateDiscountFromPointsQueryValidator()
    {
        RuleFor(x => x.Points)
            .GreaterThan(0).WithMessage("Puan değeri 0'dan büyük olmalıdır.");
    }
}
