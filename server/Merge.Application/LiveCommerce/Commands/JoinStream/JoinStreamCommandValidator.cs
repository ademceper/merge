using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.JoinStream;

public class JoinStreamCommandValidator : AbstractValidator<JoinStreamCommand>
{
    public JoinStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");

        RuleFor(x => x)
            .Must(x => x.UserId.HasValue || !string.IsNullOrWhiteSpace(x.GuestId))
            .WithMessage("UserId veya GuestId gereklidir.");

        RuleFor(x => x.GuestId)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.GuestId))
            .WithMessage("Guest ID en fazla 100 karakter olabilir.");
    }
}
