using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Security.Commands.CreateOrderVerification;

public class CreateOrderVerificationCommandValidator : AbstractValidator<CreateOrderVerificationCommand>
{
    public CreateOrderVerificationCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required");

        RuleFor(x => x.VerificationType)
            .NotEmpty().WithMessage("VerificationType is required")
            .MaximumLength(100).WithMessage("VerificationType cannot exceed 100 characters")
            .Must(vt => Enum.TryParse<VerificationType>(vt, true, out _))
            .WithMessage("Invalid VerificationType. Must be a valid VerificationType enum value.");

        RuleFor(x => x.VerificationMethod)
            .MaximumLength(100).WithMessage("VerificationMethod cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.VerificationMethod));

        RuleFor(x => x.VerificationNotes)
            .MaximumLength(2000).WithMessage("VerificationNotes cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.VerificationNotes));
    }
}
