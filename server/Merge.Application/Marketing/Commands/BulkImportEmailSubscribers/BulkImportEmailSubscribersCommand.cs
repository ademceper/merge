using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.BulkImportEmailSubscribers;

public record BulkImportEmailSubscribersCommand(
    List<CreateEmailSubscriberDto> Subscribers) : IRequest<int>;
