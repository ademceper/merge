using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetMyReferralCode;

public class GetMyReferralCodeQueryValidator : AbstractValidator<GetMyReferralCodeQuery>
{
    public GetMyReferralCodeQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
