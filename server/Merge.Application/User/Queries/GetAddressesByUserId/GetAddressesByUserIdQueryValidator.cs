using FluentValidation;

namespace Merge.Application.User.Queries.GetAddressesByUserId;

public class GetAddressesByUserIdQueryValidator : AbstractValidator<GetAddressesByUserIdQuery>
{
    public GetAddressesByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
