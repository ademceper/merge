using FluentValidation;

namespace Merge.Application.Catalog.Commands.UpdateLastCountDate;

public class UpdateLastCountDateCommandValidator : AbstractValidator<UpdateLastCountDateCommand>
{
    public UpdateLastCountDateCommandValidator()
    {
        RuleFor(x => x.InventoryId)
            .NotEmpty()
            .WithMessage("Envanter ID'si zorunludur.");
    }
}

