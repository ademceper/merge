using FluentValidation;

namespace Merge.Application.Logistics.Commands.ActivateWarehouse;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class ActivateWarehouseCommandValidator : AbstractValidator<ActivateWarehouseCommand>
{
    public ActivateWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Depo ID'si zorunludur.");
    }
}

