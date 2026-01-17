using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.User.Queries.GetUserSessions;

public class GetUserSessionsQueryValidator(IOptions<UserSettings> userSettings) : AbstractValidator<GetUserSessionsQuery>
{
    private readonly UserSettings settings = userSettings.Value;

    public GetUserSessionsQueryValidator() : this(Options.Create(new UserSettings()))
    {

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Days)
            .GreaterThan(0)
            .WithMessage("Gün sayısı 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(settings.Activity.MaxSessionDays)
            .WithMessage($"Gün sayısı en fazla {settings.Activity.MaxSessionDays} olabilir.");
    }
}
