using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Security.Commands.LogSecurityEvent;

public class LogSecurityEventCommandValidator : AbstractValidator<LogSecurityEventCommand>
{
    public LogSecurityEventCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.EventType)
            .NotEmpty().WithMessage("EventType is required")
            .MaximumLength(100).WithMessage("EventType cannot exceed 100 characters")
            .Must(et => Enum.TryParse<SecurityEventType>(et, true, out _))
            .WithMessage("Invalid EventType. Must be a valid SecurityEventType enum value.");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required")
            .MaximumLength(50).WithMessage("Severity cannot exceed 50 characters")
            .Must(s => Enum.TryParse<SecurityEventSeverity>(s, true, out _))
            .WithMessage("Invalid Severity. Must be a valid SecurityEventSeverity enum value.");

        RuleFor(x => x.IpAddress)
            .MaximumLength(50).WithMessage("IpAddress cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.IpAddress));

        RuleFor(x => x.UserAgent)
            .MaximumLength(500).WithMessage("UserAgent cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.UserAgent));

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.DeviceFingerprint)
            .MaximumLength(200).WithMessage("DeviceFingerprint cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.DeviceFingerprint));
    }
}
