using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Commands.UpdateShippingStatus;

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

