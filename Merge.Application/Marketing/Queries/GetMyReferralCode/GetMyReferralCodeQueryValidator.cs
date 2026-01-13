using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetMyReferralCode;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetMyReferralCodeQueryValidator() : AbstractValidator<GetMyReferralCodeQuery>
{
    public GetMyReferralCodeQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
