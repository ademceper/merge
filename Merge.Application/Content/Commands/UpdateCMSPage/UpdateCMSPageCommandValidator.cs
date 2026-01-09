using FluentValidation;

namespace Merge.Application.Content.Commands.UpdateCMSPage;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UpdateCMSPageCommandValidator : AbstractValidator<UpdateCMSPageCommand>
{
    public UpdateCMSPageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("CMS sayfası ID'si zorunludur.");

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .MinimumLength(2)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Başlık en az 2, en fazla 200 karakter olmalıdır.");

        RuleFor(x => x.Content)
            .MaximumLength(50000)
            .MinimumLength(10)
            .When(x => !string.IsNullOrEmpty(x.Content))
            .WithMessage("İçerik en az 10, en fazla 50000 karakter olmalıdır.");

        RuleFor(x => x.Excerpt)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Excerpt))
            .WithMessage("Özet en fazla 500 karakter olabilir.");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || status == "Draft" || status == "Published" || status == "Archived")
            .When(x => !string.IsNullOrEmpty(x.Status))
            .WithMessage("Durum geçerli bir değer olmalıdır (Draft, Published, Archived).");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DisplayOrder.HasValue)
            .WithMessage("Görüntüleme sırası 0 veya daha büyük olmalıdır.");
    }
}

