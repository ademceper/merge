using FluentValidation;

namespace Merge.Application.Search.Commands.RecordSearch;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class RecordSearchCommandValidator : AbstractValidator<RecordSearchCommand>
{
    public RecordSearchCommandValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .WithMessage("Arama terimi boş olamaz.")
            .MaximumLength(200)
            .WithMessage("Arama terimi en fazla 200 karakter olabilir.");

        RuleFor(x => x.ResultCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Sonuç sayısı 0 veya daha büyük olmalıdır.");
    }
}
