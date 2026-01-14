using FluentValidation;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Commands.UpdatePageBuilder;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class UpdatePageBuilderCommandValidator : AbstractValidator<UpdatePageBuilderCommand>
{
    public UpdatePageBuilderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("İsim en fazla 200 karakter olabilir")
            .MinimumLength(2).WithMessage("İsim en az 2 karakter olmalıdır")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Slug)
            .MaximumLength(200).WithMessage("Slug en fazla 200 karakter olabilir")
            .MinimumLength(2).WithMessage("Slug en az 2 karakter olmalıdır")
            .When(x => !string.IsNullOrEmpty(x.Slug));

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir")
            .MinimumLength(2).WithMessage("Başlık en az 2 karakter olmalıdır")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Content)
            .MaximumLength(50000).WithMessage("İçerik en fazla 50000 karakter olabilir")
            .MinimumLength(10).WithMessage("İçerik en az 10 karakter olmalıdır")
            .When(x => !string.IsNullOrEmpty(x.Content));

        RuleFor(x => x.Template)
            .MaximumLength(100).WithMessage("Template en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Template));

        RuleFor(x => x.PageType)
            .MaximumLength(50).WithMessage("Sayfa tipi en fazla 50 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.PageType));

        RuleFor(x => x.MetaTitle)
            .MaximumLength(200).WithMessage("Meta başlık en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.MetaTitle));

        RuleFor(x => x.MetaDescription)
            .MaximumLength(500).WithMessage("Meta açıklama en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.MetaDescription));

        RuleFor(x => x.OgImageUrl)
            .Must(BeValidUrl).WithMessage("Geçerli bir URL giriniz")
            .When(x => !string.IsNullOrEmpty(x.OgImageUrl));
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

