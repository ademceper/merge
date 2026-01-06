using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Commands.UpdateBanner;

public class UpdateBannerCommandHandler : IRequestHandler<UpdateBannerCommand, BannerDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateBannerCommandHandler> _logger;

    public UpdateBannerCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateBannerCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BannerDto> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Banner guncelleniyor. BannerId: {BannerId}, Title: {Title}",
            request.Id, request.Title);

        try
        {
            var banner = await _context.Set<Banner>()
                .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (banner == null)
            {
                _logger.LogWarning("Banner bulunamadi. BannerId: {BannerId}", request.Id);
                throw new NotFoundException("Banner", request.Id);
            }

            banner.Title = request.Title;
            banner.Description = request.Description;
            banner.ImageUrl = request.ImageUrl;
            banner.LinkUrl = request.LinkUrl;
            banner.Position = request.Position;
            banner.SortOrder = request.SortOrder;
            banner.IsActive = request.IsActive;
            banner.StartDate = request.StartDate;
            banner.EndDate = request.EndDate;
            banner.CategoryId = request.CategoryId;
            banner.ProductId = request.ProductId;
            banner.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Banner guncellendi. BannerId: {BannerId}, Title: {Title}",
                banner.Id, banner.Title);

            return _mapper.Map<BannerDto>(banner);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Banner guncelleme hatasi. BannerId: {BannerId}, Title: {Title}",
                request.Id, request.Title);
            throw;
        }
    }
}
