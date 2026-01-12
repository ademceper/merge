using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Commands.DeleteAddress;

public class DeleteAddressCommandValidator : AbstractValidator<DeleteAddressCommand>
{
    public DeleteAddressCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Adres ID'si zorunludur.");
    }
}
