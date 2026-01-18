using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CompleteOrderSplit;

public class CompleteOrderSplitCommandValidator : AbstractValidator<CompleteOrderSplitCommand>
{
    public CompleteOrderSplitCommandValidator()
    {
        RuleFor(x => x.SplitId)
            .NotEmpty()
            .WithMessage("Sipariş bölünmesi ID'si zorunludur.");
    }
}
