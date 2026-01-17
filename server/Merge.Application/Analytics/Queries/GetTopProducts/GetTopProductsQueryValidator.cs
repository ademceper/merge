using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetTopProducts;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetTopProductsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetTopProductsQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetTopProductsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        var maxPageSize = settings.MaxPageSize;

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi zorunludur")
            .LessThan(x => x.EndDate).WithMessage("Başlangıç tarihi bitiş tarihinden önce olmalıdır")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Başlangıç tarihi gelecekte olamaz");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi zorunludur")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Bitiş tarihi gelecekte olamaz");

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Limit {maxPageSize}'den büyük olamaz");
    }
}

