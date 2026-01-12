using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Seller.Commands.GenerateInvoice;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GenerateInvoiceCommandValidator : AbstractValidator<GenerateInvoiceCommand>
{
    public GenerateInvoiceCommandValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Invoice data is required.");

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto!.SellerId)
                .NotEmpty().WithMessage("Seller ID is required.");

            RuleFor(x => x.Dto!.PeriodStart)
                .NotEmpty().WithMessage("Period start date is required.");

            RuleFor(x => x.Dto!.PeriodEnd)
                .NotEmpty().WithMessage("Period end date is required.")
                .GreaterThan(x => x.Dto!.PeriodStart).WithMessage("Period end date must be after period start date.");
        });
    }
}
