using FluentValidation;

namespace Merge.Application.Content.Commands.CreateCMSPage;

public class CreateCMSPageCommandValidator : AbstractValidator<CreateCMSPageCommand>
{
    public CreateCMSPageCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Başlık zorunludur.")
            .MaximumLength(200)
            .WithMessage("Başlık en fazla 200 karakter olabilir.")
            .MinimumLength(2)
            .WithMessage("Başlık en az 2 karakter olmalıdır.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("İçerik zorunludur.")
            .MaximumLength(50000)
            .WithMessage("İçerik en fazla 50000 karakter olabilir.");

        RuleFor(x => x.Excerpt)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Excerpt))
            .WithMessage("Özet en fazla 500 karakter olabilir.");

        RuleFor(x => x.PageType)
            .MaximumLength(50)
            .WithMessage("Sayfa tipi en fazla 50 karakter olabilir.");

        RuleFor(x => x.Status)
            .Must(status => status == "Draft" || status == "Published" || status == "Archived")
            .WithMessage("Durum geçerli bir değer olmalıdır (Draft, Published, Archived).");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.MetaTitle))
            .WithMessage("Meta başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.MetaDescription))
            .WithMessage("Meta açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Görüntüleme sırası 0 veya daha büyük olmalıdır.");
    }
}

