using FluentValidation;

namespace Merge.Application.Cart.Commands.MarkCartAsRecovered;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class MarkCartAsRecoveredCommandValidator : AbstractValidator<MarkCartAsRecoveredCommand>
{
    public MarkCartAsRecoveredCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Sepet ID zorunludur");
    }
}

