using FluentValidation;

namespace Merge.Application.Content.Commands.DeleteCMSPage;

public class DeleteCMSPageCommandValidator : AbstractValidator<DeleteCMSPageCommand>
{
    public DeleteCMSPageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("CMS sayfası ID'si zorunludur.");
    }
}

