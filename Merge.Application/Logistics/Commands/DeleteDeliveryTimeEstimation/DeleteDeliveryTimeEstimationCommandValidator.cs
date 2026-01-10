using FluentValidation;

namespace Merge.Application.Logistics.Commands.DeleteDeliveryTimeEstimation;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class DeleteDeliveryTimeEstimationCommandValidator : AbstractValidator<DeleteDeliveryTimeEstimationCommand>
{
    public DeleteDeliveryTimeEstimationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Teslimat süresi tahmini ID'si zorunludur.");
    }
}

