using FluentValidation;
using Merge.Domain.Modules.Support;

namespace Merge.Application.Support.Commands.DeleteFaq;

public class DeleteFaqCommandValidator : AbstractValidator<DeleteFaqCommand>
{
    public DeleteFaqCommandValidator()
    {
        RuleFor(x => x.FaqId)
            .NotEmpty().WithMessage("FAQ ID boş olamaz");
    }
}
