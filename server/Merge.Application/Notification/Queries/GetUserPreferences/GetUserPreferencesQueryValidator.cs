using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetUserPreferences;

/// <summary>
/// Get User Preferences Query Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class GetUserPreferencesQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetUserPreferencesQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetUserPreferencesQueryValidator() : this(Options.Create(new PaginationSettings()))
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
            .LessThanOrEqualTo(settings.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir.");
    }
}
