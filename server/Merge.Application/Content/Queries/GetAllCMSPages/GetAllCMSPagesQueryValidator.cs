using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Content.Queries.GetAllCMSPages;

public class GetAllCMSPagesQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetAllCMSPagesQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetAllCMSPagesQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || status == "Draft" || status == "Published" || status == "Archived")
            .WithMessage("Durum geçerli bir değer olmalıdır (Draft, Published, Archived).");

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

