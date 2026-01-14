using FluentValidation;

namespace Merge.Application.Seller.Commands.SendInvoice;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class SendInvoiceCommandValidator : AbstractValidator<SendInvoiceCommand>
{
    public SendInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Fatura ID boş olamaz");
    }
}
