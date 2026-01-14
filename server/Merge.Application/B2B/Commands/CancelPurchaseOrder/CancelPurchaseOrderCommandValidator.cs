using FluentValidation;

namespace Merge.Application.B2B.Commands.CancelPurchaseOrder;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class CancelPurchaseOrderCommandValidator : AbstractValidator<CancelPurchaseOrderCommand>
{
    public CancelPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Satın alma siparişi ID boş olamaz");
    }
}

