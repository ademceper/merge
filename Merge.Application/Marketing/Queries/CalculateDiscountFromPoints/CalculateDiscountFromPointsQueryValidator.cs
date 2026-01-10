using FluentValidation;

namespace Merge.Application.Marketing.Queries.CalculateDiscountFromPoints;

public class CalculateDiscountFromPointsQueryValidator : AbstractValidator<CalculateDiscountFromPointsQuery>
{
    public CalculateDiscountFromPointsQueryValidator()
    {
        RuleFor(x => x.Points)
            .GreaterThan(0).WithMessage("Puan değeri 0'dan büyük olmalıdır.");
    }
}
