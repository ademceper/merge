using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Commands.UpdateShippingStatus;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class UpdateShippingStatusCommandValidator : AbstractValidator<UpdateShippingStatusCommand>
{
    public UpdateShippingStatusCommandValidator()
    {
        RuleFor(x => x.ShippingId)
            .NotEmpty().WithMessage("Kargo ID'si zorunludur.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Geçerli bir kargo durumu seçiniz.");
    }
}

