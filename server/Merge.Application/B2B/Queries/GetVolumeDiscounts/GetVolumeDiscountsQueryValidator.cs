using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.B2B.Queries.GetVolumeDiscounts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetVolumeDiscountsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetVolumeDiscountsQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetVolumeDiscountsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(settings.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir");
    }
}

