using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Commands.UpdateLandingPage;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class UpdateLandingPageCommandValidator : AbstractValidator<UpdateLandingPageCommand>
{
    public UpdateLandingPageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("İsim en fazla 200 karakter olabilir")
            .MinimumLength(2).WithMessage("İsim en az 2 karakter olmalıdır")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir")
            .MinimumLength(2).WithMessage("Başlık en az 2 karakter olmalıdır")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Content)
            .MaximumLength(10000).WithMessage("İçerik en fazla 10000 karakter olabilir")
            .MinimumLength(10).WithMessage("İçerik en az 10 karakter olmalıdır")
            .When(x => !string.IsNullOrEmpty(x.Content));

        RuleFor(x => x.Template)
            .MaximumLength(100).WithMessage("Template en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Template));

        RuleFor(x => x.Status)
            .Must(BeValidStatus).WithMessage("Geçersiz durum değeri")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate).WithMessage("Başlangıç tarihi bitiş tarihinden önce olmalıdır")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.MetaTitle)
            .MaximumLength(200).WithMessage("Meta başlık en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.MetaTitle));

        RuleFor(x => x.MetaDescription)
            .MaximumLength(500).WithMessage("Meta açıklama en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.MetaDescription));

        RuleFor(x => x.OgImageUrl)
            .Must(BeValidUrl).WithMessage("Geçerli bir URL giriniz")
            .When(x => !string.IsNullOrEmpty(x.OgImageUrl));

        RuleFor(x => x.TrafficSplit)
            .InclusiveBetween(0, 100).WithMessage("Trafik bölünmesi 0 ile 100 arasında olmalıdır")
            .When(x => x.TrafficSplit.HasValue);
    }

    private static bool BeValidStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return false;
        return Enum.TryParse<ContentStatus>(status, true, out _);
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

