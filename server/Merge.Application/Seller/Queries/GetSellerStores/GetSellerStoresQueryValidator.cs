using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Seller.Queries.GetSellerStores;

public class GetSellerStoresQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetSellerStoresQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetSellerStoresQueryValidator() : this(Options.Create(new PaginationSettings()))
    {

        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page numarası 1'den büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxPageSize).WithMessage($"Page size en fazla {settings.MaxPageSize} olabilir.");
    }
}
