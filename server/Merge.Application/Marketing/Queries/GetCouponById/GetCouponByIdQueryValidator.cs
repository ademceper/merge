using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetCouponById;

public class GetCouponByIdQueryValidator : AbstractValidator<GetCouponByIdQuery>
{
    public GetCouponByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Kupon ID'si zorunludur.");
    }
}
