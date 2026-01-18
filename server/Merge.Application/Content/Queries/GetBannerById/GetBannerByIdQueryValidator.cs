using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Content.Queries.GetBannerById;

public class GetBannerByIdQueryValidator : AbstractValidator<GetBannerByIdQuery>
{
    public GetBannerByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Banner ID'si zorunludur.");
    }
}

