using FluentValidation;

namespace Merge.Application.Logistics.Commands.DeactivateWarehouse;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class DeactivateWarehouseCommandValidator : AbstractValidator<DeactivateWarehouseCommand>
{
    public DeactivateWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Depo ID'si zorunludur.");
    }
}

