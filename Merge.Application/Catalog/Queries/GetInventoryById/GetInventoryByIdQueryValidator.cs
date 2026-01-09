using FluentValidation;

namespace Merge.Application.Catalog.Queries.GetInventoryById;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetInventoryByIdQueryValidator : AbstractValidator<GetInventoryByIdQuery>
{
    public GetInventoryByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Envanter ID'si zorunludur.");
    }
}

