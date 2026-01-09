using FluentValidation;

namespace Merge.Application.Content.Queries.GetPageBuilderById;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetPageBuilderByIdQueryValidator : AbstractValidator<GetPageBuilderByIdQuery>
{
    public GetPageBuilderByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

