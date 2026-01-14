using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetCouponByCode;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetCouponByCodeQueryValidator : AbstractValidator<GetCouponByCodeQuery>
{
    public GetCouponByCodeQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Kupon kodu zorunludur.")
            .MaximumLength(50)
            .WithMessage("Kupon kodu en fazla 50 karakter olabilir.");
    }
}
