using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteEmailTemplate;

public class DeleteEmailTemplateCommandValidator : AbstractValidator<DeleteEmailTemplateCommand>
{
    public DeleteEmailTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Template ID zorunludur.");
    }
}
