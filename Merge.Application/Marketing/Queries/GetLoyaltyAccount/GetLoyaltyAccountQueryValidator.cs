using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetLoyaltyAccount;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetLoyaltyAccountQueryValidator() : AbstractValidator<GetLoyaltyAccountQuery>
{
    public GetLoyaltyAccountQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
