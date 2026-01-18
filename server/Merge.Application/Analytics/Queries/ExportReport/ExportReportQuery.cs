using MediatR;

namespace Merge.Application.Analytics.Queries.ExportReport;

public record ExportReportQuery(
    Guid Id,
    Guid UserId
) : IRequest<byte[]?>;

