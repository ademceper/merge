using FluentValidation;

namespace Merge.Application.Search.Commands.RecordClick;

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
