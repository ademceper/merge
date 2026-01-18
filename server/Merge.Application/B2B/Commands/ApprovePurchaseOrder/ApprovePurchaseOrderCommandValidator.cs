using FluentValidation;

namespace Merge.Application.B2B.Commands.ApprovePurchaseOrder;

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

