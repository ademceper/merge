using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteEmailTemplate;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class DeleteEmailTemplateCommandValidator : AbstractValidator<DeleteEmailTemplateCommand>
{
    public DeleteEmailTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Template ID zorunludur.");
    }
}
