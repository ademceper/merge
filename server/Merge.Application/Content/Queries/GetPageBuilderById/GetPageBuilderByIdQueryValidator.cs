using FluentValidation;

namespace Merge.Application.Content.Queries.GetPageBuilderById;

public class GetPageBuilderByIdQueryValidator : AbstractValidator<GetPageBuilderByIdQuery>
{
    public GetPageBuilderByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

