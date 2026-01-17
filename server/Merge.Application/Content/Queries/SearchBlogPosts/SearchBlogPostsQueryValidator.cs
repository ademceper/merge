using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Content.Queries.SearchBlogPosts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class SearchBlogPostsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<SearchBlogPostsQuery>
{
    private readonly PaginationSettings config = paginationSettings.Value;

    public SearchBlogPostsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
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
            .LessThanOrEqualTo(config.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {config.MaxPageSize} olabilir.");
    }
}

