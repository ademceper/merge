using FluentValidation;
using Merge.Application.Order.Commands.SplitOrder;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.SplitOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Validation
public class SplitOrderCommandValidator : AbstractValidator<SplitOrderCommand>
{
    public SplitOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Sipariş ID boş olamaz.");

        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Split bilgileri boş olamaz.");

        RuleFor(x => x.Dto!.Items)
            .NotEmpty().WithMessage("En az bir sipariş kalemi belirtilmelidir.");

        RuleForEach(x => x.Dto!.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.OrderItemId)
                    .NotEmpty().WithMessage("Sipariş kalemi ID boş olamaz.");
                
                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");
            });
    }
}
