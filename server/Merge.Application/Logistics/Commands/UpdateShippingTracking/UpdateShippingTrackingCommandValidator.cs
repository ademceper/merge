using FluentValidation;

namespace Merge.Application.Logistics.Commands.UpdateShippingTracking;

public class UpdateShippingTrackingCommandValidator : AbstractValidator<UpdateShippingTrackingCommand>
{
    public UpdateShippingTrackingCommandValidator()
    {
        RuleFor(x => x.ShippingId)
            .NotEmpty().WithMessage("Kargo ID'si zorunludur.");

        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage("Takip numarası zorunludur.")
            .MaximumLength(100).WithMessage("Takip numarası en fazla 100 karakter olabilir.")
            .MinimumLength(1).WithMessage("Takip numarası gereklidir.");
    }
}

