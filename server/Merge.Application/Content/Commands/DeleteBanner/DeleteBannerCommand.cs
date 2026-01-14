using MediatR;

namespace Merge.Application.Content.Commands.DeleteBanner;

public record DeleteBannerCommand(Guid Id) : IRequest<bool>;
