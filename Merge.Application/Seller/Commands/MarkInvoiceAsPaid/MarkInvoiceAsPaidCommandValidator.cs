using FluentValidation;

namespace Merge.Application.Seller.Commands.MarkInvoiceAsPaid;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class MarkInvoiceAsPaidCommandValidator : AbstractValidator<MarkInvoiceAsPaidCommand>
{
    public MarkInvoiceAsPaidCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");
    }
}
