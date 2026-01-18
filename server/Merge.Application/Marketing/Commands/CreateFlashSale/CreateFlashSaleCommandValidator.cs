using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Marketing.Commands.CreateFlashSale;

public class CreateFlashSaleCommandValidator : AbstractValidator<CreateFlashSaleCommand>
{
    public CreateFlashSaleCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık zorunludur.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.")
            .MinimumLength(2).WithMessage("Başlık en az 2 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi zorunludur.")
            .GreaterThanOrEqualTo(DateTime.UtcNow).WithMessage("Başlangıç tarihi geçmişte olamaz.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi zorunludur.")
            .GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        RuleFor(x => x.BannerImageUrl)
            .MaximumLength(500).WithMessage("Banner görsel URL'i en fazla 500 karakter olabilir.")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.BannerImageUrl))
            .WithMessage("Geçerli bir URL giriniz.");
    }
}
