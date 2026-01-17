using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Content.Queries.GetRecentBlogPosts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetRecentBlogPostsQueryValidator(IOptions<ContentSettings> contentSettings) : AbstractValidator<GetRecentBlogPostsQuery>
{
    private readonly ContentSettings config = contentSettings.Value;

    public GetRecentBlogPostsQueryValidator() : this(Options.Create(new ContentSettings()))
    {
        RuleFor(x => x.Count)
            .GreaterThan(0)
            .WithMessage("Sayı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxRecentPostsCount)
            .WithMessage($"Sayı en fazla {config.MaxRecentPostsCount} olabilir.");
    }
}

