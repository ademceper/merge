using FluentValidation;

namespace Merge.Application.Search.Commands.RecordClick;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class RecordClickCommandValidator : AbstractValidator<RecordClickCommand>
{
    public RecordClickCommandValidator()
    {
        RuleFor(x => x.SearchHistoryId)
            .NotEmpty()
            .WithMessage("Search history ID'si boş olamaz.");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si boş olamaz.");
    }
}
