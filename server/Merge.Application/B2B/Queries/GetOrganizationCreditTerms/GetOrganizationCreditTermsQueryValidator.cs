using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.B2B.Queries.GetOrganizationCreditTerms;

public class GetOrganizationCreditTermsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetOrganizationCreditTermsQuery>
{
    private readonly PaginationSettings settings = paginationSettings.Value;

    public GetOrganizationCreditTermsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organizasyon ID boş olamaz");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(settings.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {settings.MaxPageSize} olabilir");
    }
}

