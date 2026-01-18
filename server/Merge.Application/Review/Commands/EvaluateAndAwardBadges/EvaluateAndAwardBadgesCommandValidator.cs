using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.EvaluateAndAwardBadges;

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
