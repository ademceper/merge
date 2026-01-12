using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.ApproveAnswer;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ApproveAnswerCommandValidator : AbstractValidator<ApproveAnswerCommand>
{
    public ApproveAnswerCommandValidator()
    {
        RuleFor(x => x.AnswerId)
            .NotEmpty().WithMessage("Cevap ID boş olamaz.");
    }
}
