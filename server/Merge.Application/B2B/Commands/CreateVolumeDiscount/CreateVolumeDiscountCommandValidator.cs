using FluentValidation;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.CreateVolumeDiscount;

public class CreateVolumeDiscountCommandValidator : AbstractValidator<CreateVolumeDiscountCommand>
{
    public CreateVolumeDiscountCommandValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull().WithMessage("İndirim verisi boş olamaz");

        RuleFor(x => x.Dto.MinQuantity)
            .GreaterThan(0).WithMessage("Minimum miktar 0'dan büyük olmalıdır");

        RuleFor(x => x.Dto.DiscountPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("İndirim yüzdesi 0'dan küçük olamaz")
            .LessThanOrEqualTo(100).WithMessage("İndirim yüzdesi 100'den büyük olamaz");

        RuleFor(x => x.Dto)
            .Must(dto => !dto.StartDate.HasValue || !dto.EndDate.HasValue || dto.EndDate.Value > dto.StartDate.Value)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");

        RuleFor(x => x.Dto)
            .Must(dto => !dto.MaxQuantity.HasValue || dto.MaxQuantity.Value >= dto.MinQuantity)
            .WithMessage("Maksimum miktar minimum miktardan küçük olamaz");
    }
}

