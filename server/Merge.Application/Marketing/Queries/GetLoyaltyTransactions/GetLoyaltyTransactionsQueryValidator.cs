using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetLoyaltyTransactions;

public class GetLoyaltyTransactionsQueryValidator : AbstractValidator<GetLoyaltyTransactionsQuery>
{
    public GetLoyaltyTransactionsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Days)
            .GreaterThan(0).WithMessage("Gün sayısı 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(365).WithMessage("Gün sayısı en fazla 365 olabilir.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir.");
    }
}
