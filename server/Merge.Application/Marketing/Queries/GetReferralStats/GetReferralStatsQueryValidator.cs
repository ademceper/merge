using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetReferralStats;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetReferralStatsQueryValidator : AbstractValidator<GetReferralStatsQuery>
{
    public GetReferralStatsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
