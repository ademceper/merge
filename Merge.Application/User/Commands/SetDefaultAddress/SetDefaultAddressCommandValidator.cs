using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Commands.SetDefaultAddress;

public class SetDefaultAddressCommandValidator : AbstractValidator<SetDefaultAddressCommand>
{
    public SetDefaultAddressCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Adres ID'si zorunludur.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
