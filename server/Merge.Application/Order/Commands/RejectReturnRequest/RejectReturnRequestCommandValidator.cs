using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.RejectReturnRequest;

public class RejectReturnRequestCommandValidator : AbstractValidator<RejectReturnRequestCommand>
{
    public RejectReturnRequestCommandValidator()
    {
        RuleFor(x => x.ReturnRequestId)
            .NotEmpty()
            .WithMessage("İade talebi ID'si zorunludur.");

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage("Red nedeni en fazla 1000 karakter olabilir.");
    }
}
