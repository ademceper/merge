using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetTemplates;


public class GetTemplatesQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetTemplatesQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetTemplatesQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Geçerli bir bildirim tipi seçiniz.")
            .When(x => x.Type.HasValue);
    }
}
