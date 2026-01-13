using FluentValidation;

namespace Merge.Application.Marketing.Commands.PurchaseGiftCard;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class PurchaseGiftCardCommandValidator : AbstractValidator<PurchaseGiftCardCommand>
{
    public PurchaseGiftCardCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Hediye kartı tutarı 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(10000).WithMessage("Hediye kartı tutarı en fazla 10000 olabilir.");

        RuleFor(x => x.Message)
            .MaximumLength(500).WithMessage("Mesaj en fazla 500 karakter olabilir.");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).When(x => x.ExpiresAt.HasValue)
            .WithMessage("Son kullanma tarihi gelecekte olmalıdır.");
    }
}
