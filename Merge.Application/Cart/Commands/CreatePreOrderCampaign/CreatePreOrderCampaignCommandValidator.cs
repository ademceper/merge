using FluentValidation;

namespace Merge.Application.Cart.Commands.CreatePreOrderCampaign;

public class CreatePreOrderCampaignCommandValidator : AbstractValidator<CreatePreOrderCampaignCommand>
{
    public CreatePreOrderCampaignCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kampanya adı zorunludur.")
            .MaximumLength(200).WithMessage("Kampanya adı en fazla 200 karakter olabilir.")
            .MinimumLength(2).WithMessage("Kampanya adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID zorunludur.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi zorunludur.")
            .Must(date => date >= DateTime.UtcNow).WithMessage("Başlangıç tarihi gelecekte olmalıdır.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi zorunludur.")
            .GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        RuleFor(x => x.ExpectedDeliveryDate)
            .NotEmpty().WithMessage("Beklenen teslimat tarihi zorunludur.")
            .GreaterThan(x => x.EndDate).WithMessage("Beklenen teslimat tarihi bitiş tarihinden sonra olmalıdır.");

        RuleFor(x => x.MaxQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Maksimum miktar 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.DepositPercentage)
            .InclusiveBetween(0, 100).WithMessage("Depozito yüzdesi 0 ile 100 arasında olmalıdır.");

        RuleFor(x => x.SpecialPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Özel fiyat 0 veya daha büyük olmalıdır.");
    }
}

