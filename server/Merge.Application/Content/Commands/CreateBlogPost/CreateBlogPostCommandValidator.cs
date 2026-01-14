using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Commands.CreateBlogPost;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class CreateBlogPostCommandValidator : AbstractValidator<CreateBlogPostCommand>
{
    public CreateBlogPostCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Kategori ID'si zorunludur.");

        RuleFor(x => x.AuthorId)
            .NotEmpty()
            .WithMessage("Yazar ID'si zorunludur.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Blog post başlığı zorunludur.")
            .MaximumLength(200)
            .WithMessage("Blog post başlığı en fazla 200 karakter olabilir.")
            .MinimumLength(2)
            .WithMessage("Blog post başlığı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Excerpt)
            .MaximumLength(500)
            .WithMessage("Özet en fazla 500 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Blog post içeriği zorunludur.")
            .MaximumLength(50000)
            .WithMessage("Blog post içeriği en fazla 50000 karakter olabilir.")
            .MinimumLength(10)
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

