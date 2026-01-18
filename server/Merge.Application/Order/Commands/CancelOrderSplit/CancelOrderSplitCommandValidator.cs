using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CancelOrderSplit;

public class CancelOrderSplitCommandValidator : AbstractValidator<CancelOrderSplitCommand>
{
    public CancelOrderSplitCommandValidator()
    {
        RuleFor(x => x.SplitId)
            .NotEmpty()
            .WithMessage("Sipariş bölünmesi ID'si zorunludur.");
    }
}
