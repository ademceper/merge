using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetUserNotifications;

/// <summary>
/// Get User Notifications Query Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class GetUserNotificationsQueryValidator : AbstractValidator<GetUserNotificationsQuery>
{
    public GetUserNotificationsQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(paginationSettings.Value.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {paginationSettings.Value.MaxPageSize} olabilir.");
    }
}
