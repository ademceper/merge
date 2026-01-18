using FluentValidation;
using Merge.Domain.Modules.Support;

namespace Merge.Application.Support.Commands.IncrementFaqViewCount;

public class IncrementFaqViewCountCommandValidator : AbstractValidator<IncrementFaqViewCountCommand>
{
    public IncrementFaqViewCountCommandValidator()
    {
        RuleFor(x => x.FaqId)
            .NotEmpty().WithMessage("FAQ ID boş olamaz");
    }
}
