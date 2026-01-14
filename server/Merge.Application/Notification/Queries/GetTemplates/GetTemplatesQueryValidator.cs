using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetTemplates;

/// <summary>
/// Get Templates Query Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class GetTemplatesQueryValidator : AbstractValidator<GetTemplatesQuery>
{
    public GetTemplatesQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(paginationSettings.Value.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {paginationSettings.Value.MaxPageSize} olabilir.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Geçerli bir bildirim tipi seçiniz.")
            .When(x => x.Type.HasValue);
    }
}
