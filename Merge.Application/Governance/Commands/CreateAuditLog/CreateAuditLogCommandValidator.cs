using FluentValidation;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Commands.CreateAuditLog;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class CreateAuditLogCommandValidator : AbstractValidator<CreateAuditLogCommand>
{
    public CreateAuditLogCommandValidator()
    {
        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action gereklidir")
            .MaximumLength(100).WithMessage("Action en fazla 100 karakter olabilir");

        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type gereklidir")
            .MaximumLength(100).WithMessage("Entity type en fazla 100 karakter olabilir");

        RuleFor(x => x.TableName)
            .NotEmpty().WithMessage("Table name gereklidir")
            .MaximumLength(100).WithMessage("Table name en fazla 100 karakter olabilir");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("IP address gereklidir")
            .MaximumLength(50).WithMessage("IP address en fazla 50 karakter olabilir");

        RuleFor(x => x.UserAgent)
            .NotEmpty().WithMessage("User agent gereklidir")
            .MaximumLength(500).WithMessage("User agent en fazla 500 karakter olabilir");

        RuleFor(x => x.Module)
            .NotEmpty().WithMessage("Module gereklidir")
            .MaximumLength(100).WithMessage("Module en fazla 100 karakter olabilir");

        RuleFor(x => x.Severity)
            .MaximumLength(50).WithMessage("Severity en fazla 50 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Severity));

        RuleFor(x => x.UserEmail)
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz")
            .MaximumLength(200).WithMessage("User email en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.UserEmail));

        RuleFor(x => x.PrimaryKey)
            .MaximumLength(100).WithMessage("Primary key en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.PrimaryKey));

        RuleFor(x => x.ErrorMessage)
            .MaximumLength(1000).WithMessage("Error message en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.ErrorMessage));
    }
}
