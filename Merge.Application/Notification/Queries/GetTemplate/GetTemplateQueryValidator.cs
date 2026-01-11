using FluentValidation;

namespace Merge.Application.Notification.Queries.GetTemplate;

/// <summary>
/// Get Template Query Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class GetTemplateQueryValidator : AbstractValidator<GetTemplateQuery>
{
    public GetTemplateQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Şablon ID'si zorunludur.");
    }
}
