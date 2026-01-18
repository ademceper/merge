using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.MarkCartAsRecovered;

public class MarkCartAsRecoveredCommandValidator : AbstractValidator<MarkCartAsRecoveredCommand>
{
    public MarkCartAsRecoveredCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Sepet ID zorunludur");
    }
}

