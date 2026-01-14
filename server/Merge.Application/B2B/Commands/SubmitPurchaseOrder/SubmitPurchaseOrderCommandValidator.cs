using FluentValidation;

namespace Merge.Application.B2B.Commands.SubmitPurchaseOrder;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class SubmitPurchaseOrderCommandValidator : AbstractValidator<SubmitPurchaseOrderCommand>
{
    public SubmitPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Satın alma siparişi ID boş olamaz");
    }
}

