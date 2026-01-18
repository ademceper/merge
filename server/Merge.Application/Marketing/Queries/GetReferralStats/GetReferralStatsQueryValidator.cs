using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetReferralStats;

public class GetReferralStatsQueryValidator : AbstractValidator<GetReferralStatsQuery>
{
    public GetReferralStatsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
