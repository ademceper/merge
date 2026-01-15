using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Content.Queries.GetSitemapEntries;

public class GetSitemapEntriesQueryValidator : AbstractValidator<GetSitemapEntriesQuery>
{
    private readonly PaginationSettings settings;

    public GetSitemapEntriesQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        settings = paginationSettings.Value;
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Sayfa numarası 1'den büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Sayfa boyutu 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir.");
    }
}

