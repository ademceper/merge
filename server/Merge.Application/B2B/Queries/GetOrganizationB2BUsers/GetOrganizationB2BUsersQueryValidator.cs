using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.B2B.Queries.GetOrganizationB2BUsers;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetOrganizationB2BUsersQueryValidator : AbstractValidator<GetOrganizationB2BUsersQuery>
{
    public GetOrganizationB2BUsersQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organizasyon ID boş olamaz");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(paginationSettings.Value.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {paginationSettings.Value.MaxPageSize} olabilir");
    }
}

