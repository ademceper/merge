using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Commands.UpdateBlogPost;

public class UpdateBlogPostCommandValidator : AbstractValidator<UpdateBlogPostCommand>
{
    public UpdateBlogPostCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Blog post ID'si zorunludur.");

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Blog post başlığı en fazla 200 karakter olabilir.")
            .MinimumLength(2)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Blog post başlığı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Excerpt)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Excerpt))
            .WithMessage("Özet en fazla 500 karakter olabilir.");

        RuleFor(x => x.Content)
            .MaximumLength(50000)
            .When(x => !string.IsNullOrEmpty(x.Content))
            .WithMessage("Blog post içeriği en fazla 50000 karakter olabilir.")
            .MinimumLength(10)
            .When(x => !string.IsNullOrEmpty(x.Content))
            .WithMessage("Blog post içeriği en az 10 karakter olmalıdır.");

        RuleFor(x => x.FeaturedImageUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.FeaturedImageUrl))
            .WithMessage("Öne çıkan görsel URL'i en fazla 500 karakter olabilir.");

        RuleFor(x => x.Status)
            .Must(status => Enum.TryParse<Merge.Domain.Enums.ContentStatus>(status, true, out _))
            .When(x => !string.IsNullOrEmpty(x.Status))
            .WithMessage("Geçersiz blog post durumu.");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.MetaTitle))
            .WithMessage("Meta başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.MetaDescription))
            .WithMessage("Meta açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.MetaKeywords)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.MetaKeywords))
            .WithMessage("Meta anahtar kelimeler en fazla 200 karakter olabilir.");

        RuleFor(x => x.OgImageUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.OgImageUrl))
            .WithMessage("Open Graph görsel URL'i en fazla 500 karakter olabilir.");
    }
}

