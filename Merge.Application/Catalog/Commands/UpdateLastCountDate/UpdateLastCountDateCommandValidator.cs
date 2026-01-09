using FluentValidation;

namespace Merge.Application.Catalog.Commands.UpdateLastCountDate;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UpdateLastCountDateCommandValidator : AbstractValidator<UpdateLastCountDateCommand>
{
    public UpdateLastCountDateCommandValidator()
    {
        RuleFor(x => x.InventoryId)
            .NotEmpty()
            .WithMessage("Envanter ID'si zorunludur.");
    }
}

