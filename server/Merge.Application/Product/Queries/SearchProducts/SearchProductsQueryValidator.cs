using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.SearchProducts;

public class SearchProductsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<SearchProductsQuery>
{
    private readonly PaginationSettings config = paginationSettings.Value;

    public SearchProductsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .WithMessage("Arama terimi boş olamaz.")
            .MinimumLength(2)
            .WithMessage("Arama terimi en az 2 karakter olmalıdır.")
            .MaximumLength(100)
            .WithMessage("Arama terimi en fazla 100 karakter olabilir.");

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
