using FluentValidation;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetUserPreferencesSummary;


public class GetUserPreferencesSummaryQueryValidator : AbstractValidator<GetUserPreferencesSummaryQuery>
{
    public GetUserPreferencesSummaryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
