using FluentValidation;

namespace Merge.Application.Payment.Commands.SendInvoice;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class SendInvoiceCommandValidator : AbstractValidator<SendInvoiceCommand>
{
    public SendInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty()
            .WithMessage("Fatura ID'si zorunludur.");
    }
}
