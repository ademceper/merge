using FluentValidation;

namespace Merge.Application.Support.Commands.UpdateCustomerCommunicationStatus;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class UpdateCustomerCommunicationStatusCommandValidator : AbstractValidator<UpdateCustomerCommunicationStatusCommand>
{
    public UpdateCustomerCommunicationStatusCommandValidator()
    {
        RuleFor(x => x.CommunicationId)
            .NotEmpty().WithMessage("İletişim ID boş olamaz");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Durum boş olamaz")
            .Must(s => s == "Sent" || s == "Delivered" || s == "Read" || s == "Failed")
            .WithMessage("Durum Sent, Delivered, Read veya Failed olmalıdır");
    }
}
