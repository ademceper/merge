using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Analytics.Queries.GetUsers;

public class GetUsersQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetUsersQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetUsersQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        var maxPageSize = settings.MaxPageSize;

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(0).WithMessage("Sayfa boyutu 0 veya daha büyük olmalıdır")
            .LessThanOrEqualTo(maxPageSize).WithMessage($"Sayfa boyutu {maxPageSize}'den büyük olamaz");
    }
}

