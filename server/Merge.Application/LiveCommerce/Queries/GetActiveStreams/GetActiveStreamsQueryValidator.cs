using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.LiveCommerce.Queries.GetActiveStreams;

public class GetActiveStreamsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<GetActiveStreamsQuery>
{
    private readonly PaginationSettings config = paginationSettings.Value;

    public GetActiveStreamsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxPageSize)
            .WithMessage($"Sayfa boyutu en fazla {config.MaxPageSize} olabilir.");
    }
}
