using FluentValidation;

namespace Merge.Application.Order.Commands.CompleteReturnRequest;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CompleteReturnRequestCommandValidator : AbstractValidator<CompleteReturnRequestCommand>
{
    public CompleteReturnRequestCommandValidator()
    {
        RuleFor(x => x.ReturnRequestId)
            .NotEmpty()
            .WithMessage("İade talebi ID'si zorunludur.");

        RuleFor(x => x.TrackingNumber)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.TrackingNumber))
            .WithMessage("Takip numarası en fazla 100 karakter olabilir.");
    }
}
