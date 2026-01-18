using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetTemplate;


public class GetTemplateQueryValidator : AbstractValidator<GetTemplateQuery>
{
    public GetTemplateQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Şablon ID'si zorunludur.");
    }
}
