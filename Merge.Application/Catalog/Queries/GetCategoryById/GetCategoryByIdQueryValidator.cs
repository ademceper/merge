using FluentValidation;

namespace Merge.Application.Catalog.Queries.GetCategoryById;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetCategoryByIdQueryValidator : AbstractValidator<GetCategoryByIdQuery>
{
    public GetCategoryByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Kategori ID'si zorunludur.");
    }
}

