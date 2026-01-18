using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Security.Commands.CreatePaymentFraudCheck;

public class CreatePaymentFraudCheckCommandValidator : AbstractValidator<CreatePaymentFraudCheckCommand>
{
    public CreatePaymentFraudCheckCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("PaymentId is required");

        RuleFor(x => x.CheckType)
            .NotEmpty().WithMessage("CheckType is required")
            .MaximumLength(100).WithMessage("CheckType cannot exceed 100 characters")
            .Must(ct => Enum.TryParse<PaymentCheckType>(ct, true, out _))
            .WithMessage("Invalid CheckType. Must be a valid PaymentCheckType enum value.");

        RuleFor(x => x.DeviceFingerprint)
            .MaximumLength(200).WithMessage("DeviceFingerprint cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.DeviceFingerprint));

        RuleFor(x => x.IpAddress)
            .MaximumLength(50).WithMessage("IpAddress cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.IpAddress));

        RuleFor(x => x.UserAgent)
            .MaximumLength(500).WithMessage("UserAgent cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.UserAgent));
    }
}
