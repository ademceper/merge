using FluentValidation;

namespace Merge.Application.Content.Commands.PublishPageBuilder;

public class PublishPageBuilderCommandValidator : AbstractValidator<PublishPageBuilderCommand>
{
    public PublishPageBuilderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

