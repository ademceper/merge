using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CompleteOrderSplit;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CompleteOrderSplitCommandValidator : AbstractValidator<CompleteOrderSplitCommand>
{
    public CompleteOrderSplitCommandValidator()
    {
        RuleFor(x => x.SplitId)
            .NotEmpty()
            .WithMessage("Sipariş bölünmesi ID'si zorunludur.");
    }
}
