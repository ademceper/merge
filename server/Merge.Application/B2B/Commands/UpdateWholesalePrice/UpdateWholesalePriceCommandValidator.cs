using FluentValidation;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.UpdateWholesalePrice;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UpdateWholesalePriceCommandValidator : AbstractValidator<UpdateWholesalePriceCommand>
{
    public UpdateWholesalePriceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Fiyat ID boş olamaz");

        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Fiyat verisi boş olamaz");

        RuleFor(x => x.Dto)
            .Must(dto => !dto.StartDate.HasValue || !dto.EndDate.HasValue || dto.EndDate.Value > dto.StartDate.Value)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");
    }
}

