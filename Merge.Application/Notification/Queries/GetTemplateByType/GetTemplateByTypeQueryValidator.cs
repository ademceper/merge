using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetTemplateByType;

/// <summary>
/// Get Template By Type Query Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class GetTemplateByTypeQueryValidator : AbstractValidator<GetTemplateByTypeQuery>
{
    public GetTemplateByTypeQueryValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Geçerli bir bildirim tipi seçiniz.");
    }
}
