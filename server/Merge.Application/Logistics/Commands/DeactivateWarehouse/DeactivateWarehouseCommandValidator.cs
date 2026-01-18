using FluentValidation;

namespace Merge.Application.Logistics.Commands.DeactivateWarehouse;

public class DeactivateWarehouseCommandValidator : AbstractValidator<DeactivateWarehouseCommand>
{
    public DeactivateWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Depo ID'si zorunludur.");
    }
}

