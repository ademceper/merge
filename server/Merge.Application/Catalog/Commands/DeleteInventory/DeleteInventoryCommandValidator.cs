using FluentValidation;

namespace Merge.Application.Catalog.Commands.DeleteInventory;

public class DeleteInventoryCommandValidator : AbstractValidator<DeleteInventoryCommand>
{
    public DeleteInventoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Envanter ID'si zorunludur.");
    }
}

