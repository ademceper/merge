using FluentValidation;

namespace Merge.Application.B2B.Commands.RejectPurchaseOrder;

public class RejectPurchaseOrderCommandValidator : AbstractValidator<RejectPurchaseOrderCommand>
{
    public RejectPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Satın alma siparişi ID boş olamaz");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Red sebebi boş olamaz")
            .MaximumLength(500).WithMessage("Red sebebi en fazla 500 karakter olabilir");
    }
}

