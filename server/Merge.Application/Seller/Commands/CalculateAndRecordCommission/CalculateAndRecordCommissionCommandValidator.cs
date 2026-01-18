using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Seller.Commands.CalculateAndRecordCommission;

public class CalculateAndRecordCommissionCommandValidator : AbstractValidator<CalculateAndRecordCommissionCommand>
{
    public CalculateAndRecordCommissionCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order Item ID is required.");
    }
}
