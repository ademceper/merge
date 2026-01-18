using FluentValidation;

namespace Merge.Application.Seller.Commands.SendInvoice;

public class SendInvoiceCommandValidator : AbstractValidator<SendInvoiceCommand>
{
    public SendInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Fatura ID boş olamaz");
    }
}
