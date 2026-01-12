using FluentValidation;

namespace Merge.Application.Support.Commands.DeleteFaq;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class DeleteFaqCommandValidator : AbstractValidator<DeleteFaqCommand>
{
    public DeleteFaqCommandValidator()
    {
        RuleFor(x => x.FaqId)
            .NotEmpty().WithMessage("FAQ ID boş olamaz");
    }
}
