using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteAnswer;

public class DeleteAnswerCommandValidator : AbstractValidator<DeleteAnswerCommand>
{
    public DeleteAnswerCommandValidator()
    {
        RuleFor(x => x.AnswerId)
            .NotEmpty().WithMessage("Cevap ID boş olamaz.");
    }
}
