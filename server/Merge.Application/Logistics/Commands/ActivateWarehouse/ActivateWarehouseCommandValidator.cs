using FluentValidation;

namespace Merge.Application.Logistics.Commands.ActivateWarehouse;

public class ActivateWarehouseCommandValidator : AbstractValidator<ActivateWarehouseCommand>
{
    public ActivateWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Depo ID'si zorunludur.");
    }
}

