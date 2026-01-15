using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Content.Queries.GetActiveBanners;

public class GetActiveBannersQueryValidator : AbstractValidator<GetActiveBannersQuery>
{
    private readonly PaginationSettings settings;

    public GetActiveBannersQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        settings = paginationSettings.Value;
        RuleFor(x => x.Position)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Position))
            .WithMessage("Pozisyon en fazla 50 karakter olabilir.");

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

