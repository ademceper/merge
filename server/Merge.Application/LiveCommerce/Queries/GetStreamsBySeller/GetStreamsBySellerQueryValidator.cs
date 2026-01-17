using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.LiveCommerce.Queries.GetStreamsBySeller;

public class GetStreamsBySellerQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetStreamsBySellerQuery>
{
    private readonly PaginationSettings config = paginationSettings.Value;

    public GetStreamsBySellerQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Satıcı ID'si zorunludur.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {config.MaxPageSize} olabilir.");
    }
}
