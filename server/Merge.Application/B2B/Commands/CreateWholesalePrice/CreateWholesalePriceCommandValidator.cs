using FluentValidation;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.CreateWholesalePrice;

public class CreateWholesalePriceCommandValidator : AbstractValidator<CreateWholesalePriceCommand>
{
    public CreateWholesalePriceCommandValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Fiyat verisi boş olamaz");

        RuleFor(x => x.Dto.ProductId)
            .NotEmpty().WithMessage("Ürün ID boş olamaz");

        RuleFor(x => x.Dto.MinQuantity)
            .GreaterThan(0).WithMessage("Minimum miktar 0'dan büyük olmalıdır");

        RuleFor(x => x.Dto.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır");

        RuleFor(x => x.Dto)
            .Must(dto => !dto.StartDate.HasValue || !dto.EndDate.HasValue || dto.EndDate.Value > dto.StartDate.Value)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");
    }
}

