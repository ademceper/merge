using FluentValidation;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.PatchProduct;

/// <summary>
/// Validator for PatchProductCommand
/// HIGH-API-001: PATCH Support - Validation for partial updates
/// </summary>
public class PatchProductCommandValidator : AbstractValidator<PatchProductCommand>
{
    public PatchProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.PatchDto)
            .NotNull()
            .WithMessage("Güncelleme verisi zorunludur.");

        // Conditional validation - only validate fields that are provided
        When(x => x.PatchDto.Name is not null, () =>
        {
            RuleFor(x => x.PatchDto!.Name)
                .NotEmpty()
                .WithMessage("Ürün adı boş olamaz.")
                .MaximumLength(200)
                .WithMessage("Ürün adı en fazla 200 karakter olabilir.")
                .MinimumLength(2)
                .WithMessage("Ürün adı en az 2 karakter olmalıdır.");
        });

        When(x => x.PatchDto.Description is not null, () =>
        {
            RuleFor(x => x.PatchDto!.Description)
                .NotEmpty()
                .WithMessage("Ürün açıklaması boş olamaz.")
                .MaximumLength(5000)
                .WithMessage("Ürün açıklaması en fazla 5000 karakter olabilir.");
        });

        When(x => x.PatchDto.SKU is not null, () =>
        {
            RuleFor(x => x.PatchDto!.SKU)
                .NotEmpty()
                .WithMessage("SKU boş olamaz.")
                .MaximumLength(100)
                .WithMessage("SKU en fazla 100 karakter olabilir.");
        });

        When(x => x.PatchDto.Price.HasValue, () =>
        {
            RuleFor(x => x.PatchDto!.Price!.Value)
                .GreaterThan(0)
                .WithMessage("Ürün fiyatı 0'dan büyük olmalıdır.");
        });

        When(x => x.PatchDto.DiscountPrice.HasValue, () =>
        {
            RuleFor(x => x.PatchDto!.DiscountPrice!.Value)
                .GreaterThan(0)
                .WithMessage("İndirimli fiyat 0'dan büyük olmalıdır.");
        });

        When(x => x.PatchDto.StockQuantity.HasValue, () =>
        {
            RuleFor(x => x.PatchDto!.StockQuantity!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Stok miktarı negatif olamaz.");
        });

        When(x => x.PatchDto.Brand is not null, () =>
        {
            RuleFor(x => x.PatchDto!.Brand)
                .NotEmpty()
                .WithMessage("Marka adı boş olamaz.")
                .MaximumLength(100)
                .WithMessage("Marka adı en fazla 100 karakter olabilir.");
        });
    }
}
