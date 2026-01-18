using FluentValidation;

namespace Merge.Application.Logistics.Commands.StartPacking;

public class StartPackingCommandValidator : AbstractValidator<StartPackingCommand>
{
    public StartPackingCommandValidator()
    {
        RuleFor(x => x.PickPackId)
            .NotEmpty().WithMessage("Pick-pack ID'si zorunludur.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}

