using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Seller.Commands.MarkInvoiceAsPaid;

public class MarkInvoiceAsPaidCommandValidator : AbstractValidator<MarkInvoiceAsPaidCommand>
{
    public MarkInvoiceAsPaidCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");
    }
}
