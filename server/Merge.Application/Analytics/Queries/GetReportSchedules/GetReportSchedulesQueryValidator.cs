using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetReportSchedules;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetReportSchedulesQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetReportSchedulesQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetReportSchedulesQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        var maxPageSize = settings.MaxPageSize;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(0).WithMessage("Sayfa boyutu 0 veya daha büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Sayfa boyutu {maxPageSize}'den büyük olamaz");
    }
}

