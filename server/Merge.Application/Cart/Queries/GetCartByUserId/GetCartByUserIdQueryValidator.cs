using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetCartByUserId;

public class GetCartByUserIdQueryValidator : AbstractValidator<GetCartByUserIdQuery>
{
    public GetCartByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

