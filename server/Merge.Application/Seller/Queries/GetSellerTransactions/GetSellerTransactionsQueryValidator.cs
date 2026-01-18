using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Seller.Queries.GetSellerTransactions;

public class GetSellerTransactionsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetSellerTransactionsQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetSellerTransactionsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {

        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page numarası 1'den büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(settings.MaxPageSize).WithMessage($"Page size en fazla {settings.MaxPageSize} olabilir.");

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("End date must be after start date.");
        });
    }
}
