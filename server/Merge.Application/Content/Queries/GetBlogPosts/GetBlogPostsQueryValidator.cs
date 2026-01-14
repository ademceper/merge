using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Queries.GetBlogPosts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetBlogPostsQueryValidator : AbstractValidator<GetBlogPostsQuery>
{
    public GetBlogPostsQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var settings = paginationSettings.Value;

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Sayfa numarası 1'den büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Sayfa boyutu 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir.");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || Enum.TryParse<Merge.Domain.Enums.ContentStatus>(status, true, out _))
            .When(x => !string.IsNullOrEmpty(x.Status))
            .WithMessage("Geçersiz blog post durumu.");
    }
}

