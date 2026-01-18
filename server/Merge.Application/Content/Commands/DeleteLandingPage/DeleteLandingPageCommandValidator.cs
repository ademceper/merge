using FluentValidation;

namespace Merge.Application.Content.Commands.DeleteLandingPage;

public class DeleteLandingPageCommandValidator : AbstractValidator<DeleteLandingPageCommand>
{
    public DeleteLandingPageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

