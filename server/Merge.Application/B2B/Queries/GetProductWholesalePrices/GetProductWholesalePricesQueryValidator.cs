using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.B2B.Queries.GetProductWholesalePrices;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetProductWholesalePricesQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetProductWholesalePricesQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetProductWholesalePricesQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID boş olamaz");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(settings.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir");
    }
}

