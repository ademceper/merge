using FluentValidation;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Commands.GenerateInvoice;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class GenerateInvoiceCommandValidator : AbstractValidator<GenerateInvoiceCommand>
{
    public GenerateInvoiceCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Sipariş ID'si zorunludur.");
    }
}
