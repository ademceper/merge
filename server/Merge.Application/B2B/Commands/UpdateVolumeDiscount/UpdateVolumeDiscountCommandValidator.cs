using FluentValidation;

namespace Merge.Application.B2B.Commands.UpdateVolumeDiscount;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UpdateVolumeDiscountCommandValidator : AbstractValidator<UpdateVolumeDiscountCommand>
{
    public UpdateVolumeDiscountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("İndirim ID boş olamaz");

        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Güncelleme verisi boş olamaz");

        RuleFor(x => x.Dto)
            .Must(dto => !dto.StartDate.HasValue || !dto.EndDate.HasValue || dto.EndDate.Value > dto.StartDate.Value)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");

        RuleFor(x => x.Dto)
            .Must(dto => !dto.MaxQuantity.HasValue || dto.MaxQuantity.Value >= dto.MinQuantity)
            .WithMessage("Maksimum miktar minimum miktardan küçük olamaz");
    }
}

