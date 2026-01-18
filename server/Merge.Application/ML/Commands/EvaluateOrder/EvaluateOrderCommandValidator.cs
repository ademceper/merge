using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.ML.Commands.EvaluateOrder;

public class EvaluateOrderCommandValidator : AbstractValidator<EvaluateOrderCommand>
{
    public EvaluateOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");
    }
}
