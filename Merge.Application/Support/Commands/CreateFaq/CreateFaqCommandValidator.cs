using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Support.Commands.CreateFaq;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class CreateFaqCommandValidator : AbstractValidator<CreateFaqCommand>
{
    public CreateFaqCommandValidator(IOptions<SupportSettings> settings)
    {
        var supportSettings = settings.Value;

        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Soru boş olamaz")
            .MinimumLength(supportSettings.MinFaqQuestionLength).WithMessage($"Soru en az {supportSettings.MinFaqQuestionLength} karakter olmalıdır")
            .MaximumLength(supportSettings.MaxFaqQuestionLength)
            .WithMessage($"Soru en fazla {supportSettings.MaxFaqQuestionLength} karakter olmalıdır");

        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Cevap boş olamaz")
            .MinimumLength(supportSettings.MinFaqAnswerLength).WithMessage($"Cevap en az {supportSettings.MinFaqAnswerLength} karakter olmalıdır")
            .MaximumLength(supportSettings.MaxFaqAnswerLength)
            .WithMessage($"Cevap en fazla {supportSettings.MaxFaqAnswerLength} karakter olmalıdır");

        RuleFor(x => x.Category)
            .MaximumLength(supportSettings.MaxFaqCategoryLength)
            .WithMessage($"Kategori en fazla {supportSettings.MaxFaqCategoryLength} karakter olmalıdır");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(supportSettings.MinDisplayOrder).WithMessage($"Sıralama {supportSettings.MinDisplayOrder} veya daha büyük olmalıdır");
    }
}
