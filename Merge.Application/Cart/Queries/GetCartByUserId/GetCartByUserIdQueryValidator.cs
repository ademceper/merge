using FluentValidation;

namespace Merge.Application.Cart.Queries.GetCartByUserId;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetCartByUserIdQueryValidator : AbstractValidator<GetCartByUserIdQuery>
{
    public GetCartByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

