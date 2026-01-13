using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetCouponById;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetCouponByIdQueryValidator : AbstractValidator<GetCouponByIdQuery>
{
    public GetCouponByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Kupon ID'si zorunludur.");
    }
}
