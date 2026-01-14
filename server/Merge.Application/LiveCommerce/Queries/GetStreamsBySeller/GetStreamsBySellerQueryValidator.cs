using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.LiveCommerce.Queries.GetStreamsBySeller;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Magic number YASAK - Configuration kullan
public class GetStreamsBySellerQueryValidator : AbstractValidator<GetStreamsBySellerQuery>
{
    public GetStreamsBySellerQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var settings = paginationSettings.Value;

        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Satıcı ID'si zorunludur.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir.");
    }
}

