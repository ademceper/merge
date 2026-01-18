using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Catalog.Queries.GetMainCategories;

public class GetMainCategoriesQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetMainCategoriesQuery>
{
    private readonly PaginationSettings config = paginationSettings.Value;

    public GetMainCategoriesQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Sayfa numarası 1'den büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Sayfa boyutu 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {config.MaxPageSize} olabilir.");
    }
}

