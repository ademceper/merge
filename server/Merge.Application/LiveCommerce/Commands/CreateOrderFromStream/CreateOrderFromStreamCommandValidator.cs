using FluentValidation;

namespace Merge.Application.LiveCommerce.Commands.CreateOrderFromStream;

public class CreateOrderFromStreamCommandValidator : AbstractValidator<CreateOrderFromStreamCommand>
{
    public CreateOrderFromStreamCommandValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Sipariş ID'si zorunludur.");
    }
}
