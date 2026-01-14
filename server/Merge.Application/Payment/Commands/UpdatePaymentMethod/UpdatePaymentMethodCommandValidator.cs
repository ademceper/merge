using FluentValidation;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Commands.UpdatePaymentMethod;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class UpdatePaymentMethodCommandValidator : AbstractValidator<UpdatePaymentMethodCommand>
{
    public UpdatePaymentMethodCommandValidator()
    {
        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage("Ödeme yöntemi ID'si zorunludur.");

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("İsim en fazla 100 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.IconUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.IconUrl))
            .WithMessage("Icon URL en fazla 500 karakter olabilir.");

        RuleFor(x => x.MinimumAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinimumAmount.HasValue)
            .WithMessage("Minimum tutar 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.MaximumAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaximumAmount.HasValue)
            .WithMessage("Maksimum tutar 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.ProcessingFee)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ProcessingFee.HasValue)
            .WithMessage("İşlem ücreti 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.ProcessingFeePercentage)
            .InclusiveBetween(0, 100)
            .When(x => x.ProcessingFeePercentage.HasValue)
            .WithMessage("İşlem ücreti yüzdesi 0 ile 100 arasında olmalıdır.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DisplayOrder.HasValue)
            .WithMessage("Görüntüleme sırası 0 veya daha büyük olmalıdır.");
    }
}
