using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.ApproveAnswer;

public class ApproveAnswerCommandValidator : AbstractValidator<ApproveAnswerCommand>
{
    public ApproveAnswerCommandValidator()
    {
        RuleFor(x => x.AnswerId)
            .NotEmpty().WithMessage("Cevap ID boş olamaz.");
    }
}
