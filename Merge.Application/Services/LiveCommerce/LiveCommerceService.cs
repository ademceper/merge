using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.LiveCommerce;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.LiveCommerce;

namespace Merge.Application.Services.LiveCommerce;

public class LiveCommerceService : ILiveCommerceService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LiveCommerceService> _logger;
    private readonly IMapper _mapper;

    public LiveCommerceService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<LiveCommerceService> logger,
        IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<LiveStreamDto> CreateStreamAsync(CreateLiveStreamDto dto)
    {
        var stream = new LiveStream
        {
            SellerId = dto.SellerId,
            Title = dto.Title,
            Description = dto.Description,
            Status = "Scheduled",
            ScheduledStartTime = dto.ScheduledStartTime,
            StreamUrl = dto.StreamUrl,
            StreamKey = dto.StreamKey,
            ThumbnailUrl = dto.ThumbnailUrl,
            Category = dto.Category,
            Tags = dto.Tags
        };

        await _context.LiveStreams.AddAsync(stream);
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan, manuel mapping YASAK
        // ✅ PERFORMANCE: AsNoTracking + Include ile tek query'de getir
        var createdStream = await _context.LiveStreams
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == stream.Id);

        return _mapper.Map<LiveStreamDto>(createdStream);
    }

    public async Task<LiveStreamDto?> GetStreamAsync(Guid streamId)
    {
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var stream = await _context.LiveStreams
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == streamId);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return stream != null ? _mapper.Map<LiveStreamDto>(stream) : null;
    }

    public async Task<IEnumerable<LiveStreamDto>> GetStreamsAsync(Guid? sellerId = null)
    {
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var query = _context.LiveStreams
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .AsQueryable();

        if (sellerId.HasValue)
        {
            query = query.Where(s => s.SellerId == sellerId.Value);
        }

        var streams = await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return _mapper.Map<IEnumerable<LiveStreamDto>>(streams);
    }

    public async Task<LiveStreamDto> UpdateStreamAsync(Guid streamId, CreateLiveStreamDto dto)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
        {
            throw new NotFoundException("Yayın", streamId);
        }

        stream.Title = dto.Title;
        stream.Description = dto.Description;
        stream.ScheduledStartTime = dto.ScheduledStartTime;
        stream.StreamUrl = dto.StreamUrl;
        stream.StreamKey = dto.StreamKey;
        stream.ThumbnailUrl = dto.ThumbnailUrl;
        stream.Category = dto.Category;
        stream.Tags = dto.Tags;
        stream.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return await GetStreamAsync(streamId) ?? throw new NotFoundException("Yayın", streamId);
    }

    public async Task<bool> DeleteStreamAsync(Guid streamId)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null) return false;

        stream.IsDeleted = true;
        stream.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<LiveStreamDto>> GetActiveStreamsAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var streams = await _context.LiveStreams
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .Where(s => s.Status == "Live" && s.IsActive)
            .OrderByDescending(s => s.ActualStartTime)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return _mapper.Map<IEnumerable<LiveStreamDto>>(streams);
    }

    public async Task<IEnumerable<LiveStreamDto>> GetStreamsBySellerAsync(Guid sellerId)
    {
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var streams = await _context.LiveStreams
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .Where(s => s.SellerId == sellerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return _mapper.Map<IEnumerable<LiveStreamDto>>(streams);
    }

    public async Task<bool> StartStreamAsync(Guid streamId)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null) return false;

        stream.Status = "Live";
        stream.ActualStartTime = DateTime.UtcNow;
        stream.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EndStreamAsync(Guid streamId)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null) return false;

        stream.Status = "Ended";
        stream.EndTime = DateTime.UtcNow;
        stream.IsActive = false;
        stream.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<LiveStreamDto> AddProductToStreamAsync(Guid streamId, Guid productId, AddProductToStreamDto? dto = null)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
        {
            throw new NotFoundException("Yayın", streamId);
        }

        var existing = await _context.LiveStreamProducts
            .FirstOrDefaultAsync(p => p.LiveStreamId == streamId && p.ProductId == productId);

        if (existing != null)
        {
            throw new BusinessException("Ürün zaten yayına eklenmiş.");
        }

        var streamProduct = new LiveStreamProduct
        {
            LiveStreamId = streamId,
            ProductId = productId,
            DisplayOrder = dto?.DisplayOrder ?? 0,
            SpecialPrice = dto?.SpecialPrice,
            ShowcaseNotes = dto?.ShowcaseNotes
        };

        await _context.LiveStreamProducts.AddAsync(streamProduct);
        await _unitOfWork.SaveChangesAsync();

        return await GetStreamAsync(streamId) ?? throw new NotFoundException("Yayın", streamId);
    }

    public async Task<bool> ShowcaseProductAsync(Guid streamId, Guid productId)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var streamProduct = await _context.LiveStreamProducts
            .FirstOrDefaultAsync(p => p.LiveStreamId == streamId && p.ProductId == productId);

        if (streamProduct == null) return false;

        // Unhighlight all products in this stream
        // ⚠️ Batch update için ToListAsync gerekli (tracking ile)
        var allProducts = await _context.LiveStreamProducts
            .Where(p => p.LiveStreamId == streamId)
            .ToListAsync();

        foreach (var product in allProducts)
        {
            product.IsHighlighted = false;
        }

        // Highlight the selected product
        streamProduct.IsHighlighted = true;
        streamProduct.ShowcasedAt = DateTime.UtcNow;
        streamProduct.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> JoinStreamAsync(Guid streamId, Guid? userId, string? guestId = null)
    {
        var viewer = new LiveStreamViewer
        {
            LiveStreamId = streamId,
            UserId = userId,
            GuestId = guestId,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _context.LiveStreamViewers.AddAsync(viewer);

        // Update viewer count
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream != null)
        {
            // ✅ PERFORMANCE: Database'de count (memory'de YASAK)
            stream.ViewerCount = await _context.LiveStreamViewers
                .CountAsync(v => v.LiveStreamId == streamId && v.IsActive);
            stream.TotalViewerCount++;
            if (stream.ViewerCount > stream.PeakViewerCount)
            {
                stream.PeakViewerCount = stream.ViewerCount;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LeaveStreamAsync(Guid streamId, Guid? userId, string? guestId = null)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var viewer = await _context.LiveStreamViewers
            .FirstOrDefaultAsync(v => v.LiveStreamId == streamId &&
                (userId.HasValue ? v.UserId == userId : v.GuestId == guestId) &&
                v.IsActive);

        if (viewer == null) return false;

        viewer.LeftAt = DateTime.UtcNow;
        viewer.IsActive = false;
        viewer.WatchDuration = (int)(DateTime.UtcNow - viewer.JoinedAt).TotalSeconds;
        viewer.UpdatedAt = DateTime.UtcNow;

        // Update viewer count
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream != null)
        {
            // ✅ PERFORMANCE: Database'de count (memory'de YASAK)
            stream.ViewerCount = await _context.LiveStreamViewers
                .CountAsync(v => v.LiveStreamId == streamId && v.IsActive);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<LiveStreamOrderDto> CreateOrderFromStreamAsync(Guid streamId, Guid orderId, Guid? productId = null)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new NotFoundException("Sipariş", orderId);
        }

        var streamOrder = new LiveStreamOrder
        {
            LiveStreamId = streamId,
            OrderId = orderId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow,
            OrderAmount = order.TotalAmount
        };

        await _context.LiveStreamOrders.AddAsync(streamOrder);

        // Update stream stats
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream != null)
        {
            stream.OrderCount++;
            stream.Revenue += order.TotalAmount;
        }

        // Update product stats if productId provided
        if (productId.HasValue)
        {
            var streamProduct = await _context.LiveStreamProducts
                .FirstOrDefaultAsync(p => p.LiveStreamId == streamId && p.ProductId == productId.Value);

            if (streamProduct != null)
            {
                streamProduct.OrderCount++;
            }
        }

        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<LiveStreamOrderDto>(streamOrder);
    }

    public async Task<LiveStreamStatsDto> GetStreamStatsAsync(Guid streamId)
    {
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var stream = await _context.LiveStreams
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
        {
            throw new NotFoundException("Yayın", streamId);
        }

        // ✅ PERFORMANCE: Database'de aggregation (memory'de YASAK)
        var totalViewers = await _context.LiveStreamViewers
            .CountAsync(v => v.LiveStreamId == streamId);

        var totalOrders = await _context.LiveStreamOrders
            .CountAsync(o => o.LiveStreamId == streamId);

        var totalRevenue = await _context.LiveStreamOrders
            .Where(o => o.LiveStreamId == streamId)
            .SumAsync(o => (decimal?)o.OrderAmount) ?? 0;

        return new LiveStreamStatsDto
        {
            StreamId = streamId,
            ViewerCount = stream.ViewerCount,
            PeakViewerCount = stream.PeakViewerCount,
            TotalViewerCount = totalViewers,
            OrderCount = stream.OrderCount,
            Revenue = stream.Revenue,
            TotalRevenue = totalRevenue,
            Status = stream.Status,
            Duration = stream.ActualStartTime.HasValue && stream.EndTime.HasValue
                ? (int)(stream.EndTime.Value - stream.ActualStartTime.Value).TotalSeconds
                : stream.ActualStartTime.HasValue
                    ? (int)(DateTime.UtcNow - stream.ActualStartTime.Value).TotalSeconds
                    : 0
        };
    }
}

