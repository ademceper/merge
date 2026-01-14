using FluentValidation;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Commands.GenerateInvoicePdf;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class GenerateInvoicePdfCommandValidator : AbstractValidator<GenerateInvoicePdfCommand>
{
    public GenerateInvoicePdfCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty()
            .WithMessage("Fatura ID'si zorunludur.");
    }
}
