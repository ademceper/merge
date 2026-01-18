using FluentValidation;

namespace Merge.Application.Content.Commands.DeleteSitemapEntry;

public class DeleteSitemapEntryCommandValidator : AbstractValidator<DeleteSitemapEntryCommand>
{
    public DeleteSitemapEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Sitemap entry ID'si zorunludur.");
    }
}

