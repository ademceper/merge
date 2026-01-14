using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.ApproveReturnRequest;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class ApproveReturnRequestCommandValidator : AbstractValidator<ApproveReturnRequestCommand>
{
    public ApproveReturnRequestCommandValidator()
    {
        RuleFor(x => x.ReturnRequestId)
            .NotEmpty()
            .WithMessage("İade talebi ID'si zorunludur.");
    }
}
