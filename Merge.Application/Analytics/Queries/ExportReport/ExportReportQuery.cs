using MediatR;

namespace Merge.Application.Analytics.Queries.ExportReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ExportReportQuery(
    Guid Id,
    Guid UserId
) : IRequest<byte[]?>;

