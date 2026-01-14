using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.Reorder;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class ReorderCommandValidator : AbstractValidator<ReorderCommand>
{
    public ReorderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Sipariş ID'si zorunludur.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
