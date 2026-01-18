using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.ApproveReturnRequest;

public class ApproveReturnRequestCommandValidator : AbstractValidator<ApproveReturnRequestCommand>
{
    public ApproveReturnRequestCommandValidator()
    {
        RuleFor(x => x.ReturnRequestId)
            .NotEmpty()
            .WithMessage("İade talebi ID'si zorunludur.");
    }
}
