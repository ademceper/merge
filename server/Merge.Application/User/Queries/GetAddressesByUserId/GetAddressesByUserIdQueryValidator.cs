using FluentValidation;

namespace Merge.Application.User.Queries.GetAddressesByUserId;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetAddressesByUserIdQueryValidator : AbstractValidator<GetAddressesByUserIdQuery>
{
    public GetAddressesByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
