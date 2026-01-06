using FluentValidation;

namespace Merge.Application.B2B.Commands.ApprovePurchaseOrder;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ApprovePurchaseOrderCommandValidator : AbstractValidator<ApprovePurchaseOrderCommand>
{
    public ApprovePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Satın alma siparişi ID boş olamaz");

        RuleFor(x => x.ApprovedByUserId)
            .NotEmpty().WithMessage("Onaylayan kullanıcı ID boş olamaz");
    }
}

