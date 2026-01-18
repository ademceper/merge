using FluentValidation;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetEnabledChannels;


public class GetEnabledChannelsQueryValidator : AbstractValidator<GetEnabledChannelsQuery>
{
    public GetEnabledChannelsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.NotificationType)
            .IsInEnum()
            .WithMessage("Geçerli bir bildirim tipi seçiniz.");
    }
}
