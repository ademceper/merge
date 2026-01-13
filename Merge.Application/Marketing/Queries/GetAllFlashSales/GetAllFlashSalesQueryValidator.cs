using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetAllFlashSales;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetAllFlashSalesQueryValidator : AbstractValidator<GetAllFlashSalesQuery>
{
    public GetAllFlashSalesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir.");
    }
}
