using FluentValidation;

namespace Merge.Application.Logistics.Commands.StartPicking;

public class StartPickingCommandValidator : AbstractValidator<StartPickingCommand>
{
    public StartPickingCommandValidator()
    {
        RuleFor(x => x.PickPackId)
            .NotEmpty().WithMessage("Pick-pack ID'si zorunludur.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}

