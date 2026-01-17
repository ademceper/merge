using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Support.Commands.CreateFaq;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class CreateFaqCommandValidator(IOptions<SupportSettings> settings) : AbstractValidator<CreateFaqCommand>
{
    private readonly SupportSettings config = settings.Value;

    public CreateFaqCommandValidator() : this(Options.Create(new SupportSettings()))
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Soru boş olamaz")
            .MinimumLength(config.MinFaqQuestionLength).WithMessage($"Soru en az {config.MinFaqQuestionLength} karakter olmalıdır")
            .MaximumLength(config.MaxFaqQuestionLength)
            .WithMessage($"Soru en fazla {config.MaxFaqQuestionLength} karakter olmalıdır");

        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Cevap boş olamaz")
            .MinimumLength(config.MinFaqAnswerLength).WithMessage($"Cevap en az {config.MinFaqAnswerLength} karakter olmalıdır")
            .MaximumLength(config.MaxFaqAnswerLength)
            .WithMessage($"Cevap en fazla {config.MaxFaqAnswerLength} karakter olmalıdır");

        RuleFor(x => x.Category)
            .MaximumLength(config.MaxFaqCategoryLength)
            .WithMessage($"Kategori en fazla {config.MaxFaqCategoryLength} karakter olmalıdır");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(config.MinDisplayOrder).WithMessage($"Sıralama {config.MinDisplayOrder} veya daha büyük olmalıdır");
    }
}
