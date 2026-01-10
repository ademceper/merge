using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.BulkImportEmailSubscribers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record BulkImportEmailSubscribersCommand(
    List<CreateEmailSubscriberDto> Subscribers) : IRequest<int>;
