using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Commands.CreateTransaction;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required.");

        // ✅ ARCHITECTURE: Enum kullanımı (string TransactionType yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("Transaction type must be a valid enum value.");

        RuleFor(x => x.Amount)
            .NotEqual(0).WithMessage("Amount cannot be zero.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
