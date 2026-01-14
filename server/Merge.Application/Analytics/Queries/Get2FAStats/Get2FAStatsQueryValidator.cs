using FluentValidation;

namespace Merge.Application.Analytics.Queries.Get2FAStats;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class Get2FAStatsQueryValidator : AbstractValidator<Get2FAStatsQuery>
{
    public Get2FAStatsQueryValidator()
    {
        // Get2FAStatsQuery parametre almadığı için validation gerekmez
        // Ancak FluentValidation pattern'i için boş validator oluşturuyoruz
    }
}

