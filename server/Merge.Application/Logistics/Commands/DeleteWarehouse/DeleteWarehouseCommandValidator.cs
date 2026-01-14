using FluentValidation;

namespace Merge.Application.Logistics.Commands.DeleteWarehouse;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class DeleteWarehouseCommandValidator : AbstractValidator<DeleteWarehouseCommand>
{
    public DeleteWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Depo ID'si zorunludur.");
    }
}

