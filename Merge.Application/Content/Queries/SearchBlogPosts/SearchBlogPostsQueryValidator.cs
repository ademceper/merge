using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Content.Queries.SearchBlogPosts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class SearchBlogPostsQueryValidator : AbstractValidator<SearchBlogPostsQuery>
{
    public SearchBlogPostsQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var settings = paginationSettings.Value;

        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Arama sorgusu zorunludur.")
            .MinimumLength(2)
            .WithMessage("Arama sorgusu en az 2 karakter olmalıdır.")
            .MaximumLength(200)
            .WithMessage("Arama sorgusu en fazla 200 karakter olabilir.");

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

