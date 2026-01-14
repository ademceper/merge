using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteEmailTemplate;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class DeleteEmailTemplateCommandValidator : AbstractValidator<DeleteEmailTemplateCommand>
{
    public DeleteEmailTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Template ID zorunludur.");
    }
}
