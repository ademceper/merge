using FluentValidation;

namespace Merge.Application.Review.Commands.EvaluateAndAwardBadges;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class EvaluateAndAwardBadgesCommandValidator : AbstractValidator<EvaluateAndAwardBadgesCommand>
{
    public EvaluateAndAwardBadgesCommandValidator()
    {
        // SellerId optional - eğer verilmişse boş olmamalı
        RuleFor(x => x.SellerId)
            .NotEmpty()
            .When(x => x.SellerId.HasValue)
            .WithMessage("Satıcı ID'si geçerli bir GUID olmalıdır.");
    }
}
