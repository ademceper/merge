using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetUnreadCount;


public class GetUnreadCountQueryValidator : AbstractValidator<GetUnreadCountQuery>
{
    public GetUnreadCountQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
