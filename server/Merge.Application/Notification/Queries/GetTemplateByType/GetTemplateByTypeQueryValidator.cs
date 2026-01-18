using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetTemplateByType;


public class GetTemplateByTypeQueryValidator : AbstractValidator<GetTemplateByTypeQuery>
{
    public GetTemplateByTypeQueryValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Geçerli bir bildirim tipi seçiniz.");
    }
}
