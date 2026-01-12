using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetUnreadCount;

/// <summary>
/// Get Unread Count Query Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class GetUnreadCountQueryValidator : AbstractValidator<GetUnreadCountQuery>
{
    public GetUnreadCountQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
