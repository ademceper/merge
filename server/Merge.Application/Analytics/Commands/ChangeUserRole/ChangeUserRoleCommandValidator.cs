using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Analytics.Commands.ChangeUserRole;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Rol zorunludur")
            .Must(role => role == "Admin" || role == "Manager" || role == "User" || role == "Seller")
            .WithMessage("Rol 'Admin', 'Manager', 'User' veya 'Seller' olmalıdır");
    }
}

