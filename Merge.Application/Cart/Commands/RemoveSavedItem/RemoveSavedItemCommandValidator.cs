using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.RemoveSavedItem;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class RemoveSavedItemCommandValidator : AbstractValidator<RemoveSavedItemCommand>
{
    public RemoveSavedItemCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Öğe ID zorunludur");
    }
}

