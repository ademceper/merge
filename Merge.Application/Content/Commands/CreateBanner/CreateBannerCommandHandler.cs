using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Commands.CreateBanner;

public class CreateBannerCommandHandler : IRequestHandler<CreateBannerCommand, BannerDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBannerCommandHandler> _logger;

    public CreateBannerCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateBannerCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BannerDto> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Banner olusturuluyor. Title: {Title}, Position: {Position}",
            request.Title, request.Position);

        try
        {
            var banner = new Banner
            {
                Title = request.Title,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                LinkUrl = request.LinkUrl,
                Position = request.Position,
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CategoryId = request.CategoryId,
                ProductId = request.ProductId
            };

            await _context.Set<Banner>().AddAsync(banner, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Banner olusturuldu. BannerId: {BannerId}, Title: {Title}",
                banner.Id, banner.Title);

            return _mapper.Map<BannerDto>(banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Banner olusturma hatasi. Title: {Title}, Position: {Position}",
                request.Title, request.Position);
            throw;
        }
    }
}
