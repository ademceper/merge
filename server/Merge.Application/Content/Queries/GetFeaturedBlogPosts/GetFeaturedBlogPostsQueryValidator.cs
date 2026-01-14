using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Content.Queries.GetFeaturedBlogPosts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetFeaturedBlogPostsQueryValidator : AbstractValidator<GetFeaturedBlogPostsQuery>
{
    public GetFeaturedBlogPostsQueryValidator(IOptions<ContentSettings> contentSettings)
    {
        var settings = contentSettings.Value;

        RuleFor(x => x.Count)
            .GreaterThan(0)
            .WithMessage("Sayı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxFeaturedPostsCount)
            .WithMessage($"Sayı en fazla {settings.MaxFeaturedPostsCount} olabilir.");
    }
}

