using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.ClearSavedItems;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ClearSavedItemsCommandValidator : AbstractValidator<ClearSavedItemsCommand>
{
    public ClearSavedItemsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

