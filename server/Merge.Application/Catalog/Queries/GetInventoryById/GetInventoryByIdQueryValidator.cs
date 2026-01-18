using FluentValidation;

namespace Merge.Application.Catalog.Queries.GetInventoryById;

public class GetInventoryByIdQueryValidator : AbstractValidator<GetInventoryByIdQuery>
{
    public GetInventoryByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Envanter ID'si zorunludur.");
    }
}

