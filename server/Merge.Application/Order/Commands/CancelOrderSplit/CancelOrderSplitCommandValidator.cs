using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CancelOrderSplit;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CancelOrderSplitCommandValidator : AbstractValidator<CancelOrderSplitCommand>
{
    public CancelOrderSplitCommandValidator()
    {
        RuleFor(x => x.SplitId)
            .NotEmpty()
            .WithMessage("Sipariş bölünmesi ID'si zorunludur.");
    }
}
