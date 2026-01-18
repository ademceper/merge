using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Commands.CreateStockMovement;

public class CreateStockMovementCommandValidator : AbstractValidator<CreateStockMovementCommand>
{
    public CreateStockMovementCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Depo ID'si zorunludur.");

        RuleFor(x => x.MovementType)
            .IsInEnum().WithMessage("Geçerli bir hareket tipi seçiniz.");

        RuleFor(x => x.Quantity)
            .NotEqual(0).WithMessage("Miktar 0 olamaz.")
            .GreaterThan(0).When(x => x.MovementType == StockMovementType.Receipt || x.MovementType == StockMovementType.Transfer)
            .WithMessage("Giriş hareketleri için miktar pozitif olmalıdır.");
        
        RuleFor(x => x.Quantity)
            .LessThan(0).When(x => x.MovementType == StockMovementType.Sale || x.MovementType == StockMovementType.Adjustment || x.MovementType == StockMovementType.Damage || x.MovementType == StockMovementType.Lost)
            .WithMessage("Çıkış hareketleri için miktar negatif olmalıdır.");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Referans numarası en fazla 100 karakter olabilir.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notlar en fazla 2000 karakter olabilir.");

        RuleFor(x => x.PerformedBy)
            .NotEmpty().WithMessage("İşlemi yapan kullanıcı ID'si zorunludur.");
    }
}

