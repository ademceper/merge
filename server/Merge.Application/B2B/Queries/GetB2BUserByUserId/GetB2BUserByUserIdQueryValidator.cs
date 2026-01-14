using FluentValidation;

namespace Merge.Application.B2B.Queries.GetB2BUserByUserId;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetB2BUserByUserIdQueryValidator : AbstractValidator<GetB2BUserByUserIdQuery>
{
    public GetB2BUserByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz");
    }
}

