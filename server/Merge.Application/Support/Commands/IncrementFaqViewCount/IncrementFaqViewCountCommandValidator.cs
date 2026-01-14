using FluentValidation;
using Merge.Domain.Modules.Support;

namespace Merge.Application.Support.Commands.IncrementFaqViewCount;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class IncrementFaqViewCountCommandValidator : AbstractValidator<IncrementFaqViewCountCommand>
{
    public IncrementFaqViewCountCommandValidator()
    {
        RuleFor(x => x.FaqId)
            .NotEmpty().WithMessage("FAQ ID boş olamaz");
    }
}
