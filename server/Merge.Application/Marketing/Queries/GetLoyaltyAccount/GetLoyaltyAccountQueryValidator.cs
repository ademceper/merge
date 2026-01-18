using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetLoyaltyAccount;

public class GetLoyaltyAccountQueryValidator : AbstractValidator<GetLoyaltyAccountQuery>
{
    public GetLoyaltyAccountQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
