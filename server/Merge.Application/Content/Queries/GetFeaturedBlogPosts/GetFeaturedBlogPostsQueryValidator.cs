using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Content.Queries.GetFeaturedBlogPosts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetFeaturedBlogPostsQueryValidator(IOptions<ContentSettings> contentSettings) : AbstractValidator<GetFeaturedBlogPostsQuery>
{
    private readonly ContentSettings config = contentSettings.Value;

    public GetFeaturedBlogPostsQueryValidator() : this(Options.Create(new ContentSettings()))
    {
        RuleFor(x => x.Count)
            .GreaterThan(0)
            .WithMessage("Sayı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxFeaturedPostsCount)
            .WithMessage($"Sayı en fazla {config.MaxFeaturedPostsCount} olabilir.");
    }
}

