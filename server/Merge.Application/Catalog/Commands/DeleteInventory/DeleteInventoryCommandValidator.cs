using FluentValidation;

namespace Merge.Application.Catalog.Commands.DeleteInventory;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteInventoryCommandValidator : AbstractValidator<DeleteInventoryCommand>
{
    public DeleteInventoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Envanter ID'si zorunludur.");
    }
}

