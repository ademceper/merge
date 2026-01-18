using FluentValidation;

namespace Merge.Application.Marketing.Commands.RedeemPoints;

public class RedeemPointsCommandValidator : AbstractValidator<RedeemPointsCommand>
{
    public RedeemPointsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Points)
            .GreaterThan(0).WithMessage("Puan değeri 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(1000000).WithMessage("Puan değeri en fazla 1000000 olabilir.");
    }
}
