using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Content.Commands.DeleteBanner;

public class DeleteBannerCommandValidator : AbstractValidator<DeleteBannerCommand>
{
    public DeleteBannerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Banner ID'si zorunludur.");
    }
}

